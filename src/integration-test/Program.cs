using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    public class App
    {
        static readonly string _defaultInputFile = "integration-test.json";
        static readonly string _fileNotFoundError = "File not found: {0}";
        static readonly string _sleepParameterError = "Invalid sleep (millisecond) parameter: {0}\n";
        static readonly string _threadsParameterError = "Invalid number of concurrent threads parameter: {0}\n";
        public static readonly Config _config = new Config();
        public static readonly List<TaskRunner> _taskRunners = new List<TaskRunner>();
        public static Smoker.Test _smoker;

        public static void Main(string[] args)
        {
            ProcessEnvironmentVariables();

            ProcessCommandArgs(args);

            ValidateParameters();

            _smoker = new Smoker.Test(_config.FileList, _config.Host);

            // run one test iteration
            if (!_config.RunLoop && !_config.RunWeb)
            {
                if (!_smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            IWebHost host = null;

            // setup ctl c handler
            CancellationTokenSource ctCancel = new CancellationTokenSource();

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                ctCancel.Cancel();

                Console.WriteLine("Ctl-C Pressed - Starting shutdown ...");

                // give threads a chance to shutdown
                Thread.Sleep(500);

                // end the app
                Environment.Exit(0);
            };

            // run as a web server
            if (_config.RunWeb)
            {
                // use the default web host builder + startup
                IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .UseUrls("http://*:4122/");

                // build the host
                host = builder.Build();
            }

            // run tests in config.RunLoop
            if (_config.RunLoop)
            {
                TaskRunner tr;

                for (int i = 0; i < _config.Threads; i++)
                {
                    tr = new TaskRunner { TokenSource = ctCancel };

                    tr.Task = _smoker.RunLoop(i, App._config, tr.TokenSource.Token);

                    _taskRunners.Add(tr);
                }
            }

            // run the web server
            if (_config.RunWeb)
            {
                try
                {
                    Console.WriteLine($"Version: {Helium.Version.AssemblyVersion}");

                    host.Run();
                    Console.WriteLine("Web server shutdown");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Web Server Exception\n{ex}");
                }

                return;
            }

            // run the task loop
            if (_config.RunLoop && _taskRunners.Count > 0)
            {
                // Wait for all tasks to complete
                List<Task> tasks = new List<Task>();

                foreach (var trun in _taskRunners)
                {
                    tasks.Add(trun.Task);
                }

                // wait for ctrl c
                Task.WaitAll(tasks.ToArray());
            }
        }

        private static void ValidateParameters()
        {
            // make it easier to pass host
            if (!_config.Host.ToLower().StartsWith("http"))
            {
                if (_config.Host.ToLower().StartsWith("localhost"))
                {
                    _config.Host = "http://" + _config.Host;
                }
                else
                {
                    _config.Host = string.Format($"https://{_config.Host}.azurewebsites.net");
                }
            }

            if (_config.SleepMs < 0)
            {
                _config.SleepMs = 0;
            }

            if (_config.Threads > 0)
            {
                // set config.RunLoop to true
                _config.RunLoop = true;
            }

            if (_config.Threads < 0)
            {
                _config.Threads = 0;
            }

            // let's not get too crazy
            if (_config.Threads > 10)
            {
                _config.Threads = 10;
            }

            // add default files
            if (_config.FileList.Count == 0)
            {
                _config.FileList.Add(TestFileExists(_defaultInputFile));
            }
        }

        private static void ProcessCommandArgs(string[] args)
        {
            // process the command line args
            if (args.Length > 0)
            {
                if (args[0] == "--help")
                {
                    // display usage

                    Usage();
                    Environment.Exit(0);
                }

                int i = 0;

                while (i < args.Length)
                {
                    if (!args[i].StartsWith("-"))
                    {
                        Console.WriteLine($"\nInvalid argument: {args[i]}\n");
                        Usage();
                        Environment.Exit(-1);
                    }

                    // handle web (-w)
                    if (args[i] == "-w")
                    {
                        _config.RunWeb = true;
                    }

                    else if (args[i] == "-r")
                    {
                        _config.Random = true;
                    }

                    else if (args[i] == "-v")
                    {
                        _config.Verbose = true;
                    }

                    // process all other args in pairs
                    else if (i < args.Length - 1)
                    {
                        // handle host (-h http://localhost:4120/)
                        if (args[i] == "-h")
                        {
                            _config.Host = args[i + 1].Trim();
                            i++;
                        }

                        // handle input files (-i inputFile.json input2.json input3.json)
                        else if (i < args.Length - 1 && (args[i] == "-i"))
                        {
                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                string file = TestFileExists(args[i + 1]);

                                if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
                                {
                                    _config.FileList.Add(file);
                                }
                                else
                                {
                                    Console.WriteLine(_fileNotFoundError, file);
                                }

                                i++;
                            }

                            if (_config.FileList.Count == 0)
                            {
                                // exit if no files found
                                Console.WriteLine("No files found");
                                Usage();
                                Environment.Exit(-1);
                            }
                        }

                        // handle sleep (-s config.SleepMs)
                        else if (args[i] == "-s")
                        {
                            if (int.TryParse(args[i + 1], out _config.SleepMs))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine(_sleepParameterError, args[i + 1]);
                                Usage();
                                Environment.Exit(-1);
                            }
                        }

                        // handle config.Threads (-t config.Threads)
                        else if (args[i] == "-t")
                        {
                            if (int.TryParse(args[i + 1], out _config.Threads))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine(_threadsParameterError, args[i + 1]);
                                Usage();
                                Environment.Exit(-1);
                            }
                        }
                    }

                    i++;
                }
            }
        }

        private static void ProcessEnvironmentVariables()
        {
            // Get environment variables

            string env = Environment.GetEnvironmentVariable("RUNWEB");
            if (!string.IsNullOrEmpty(env))
            {
                bool.TryParse(env, out _config.RunWeb);
            }

            env = Environment.GetEnvironmentVariable("RANDOM");
            if (!string.IsNullOrEmpty(env))
            {
                bool.TryParse(env, out _config.Random);
            }

            env = Environment.GetEnvironmentVariable("VERBOSE");
            if (!string.IsNullOrEmpty(env))
            {
                bool.TryParse(env, out _config.Verbose);
            }

            env = Environment.GetEnvironmentVariable("HOST");
            if (!string.IsNullOrEmpty(env))
            {
                _config.Host = env;
            }

            env = Environment.GetEnvironmentVariable("FILES");
            if (!string.IsNullOrEmpty(env))
            {
                string[] files = env.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string f in files)
                {
                    string file = TestFileExists(f.Trim('\'', '\"', ' '));

                    if (System.IO.File.Exists(file))
                    {
                        _config.FileList.Add(file);
                    }
                    else
                    {
                        Console.WriteLine(_fileNotFoundError, file);
                    }
                }

                if (_config.FileList.Count == 0)
                {
                    // exit if no files found
                    Console.WriteLine("No files found");
                    Environment.Exit(-1);
                }
            }

            env = Environment.GetEnvironmentVariable("SLEEP");
            if (!string.IsNullOrEmpty(env))
            {
                if (!int.TryParse(env, out _config.SleepMs))
                {
                    // exit on error
                    Console.WriteLine(_sleepParameterError, env);
                    Environment.Exit(-1);
                }
            }

            env = Environment.GetEnvironmentVariable("THREADS");
            if (!string.IsNullOrEmpty(env))
            {
                if (!int.TryParse(env, out _config.Threads))
                {
                    // exit on error
                    Console.WriteLine(_threadsParameterError, env);
                    Environment.Exit(-1);
                }
            }
        }

        private static string TestFileExists(string name)
        {
            string file = name.Trim();

            if (!string.IsNullOrEmpty(file))
            {
                if (file.Contains("TestFiles"))
                {
                    if (!System.IO.File.Exists(file))
                    {
                        file = file.Replace("TestFiles/", string.Empty);
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(file))
                    {
                        file = "TestFiles/" + file;
                    }
                }
            }

            if (System.IO.File.Exists(file))
            {
                return file;
            }

            return string.Empty;
        }

        // display the usage text
        private static void Usage()
        {
            Console.WriteLine("Usage: integration-test [--help] [-i file1.json [file2.json] [file3.json] ...] [-h hostUrl] [-s sleepMs] [-t numberOfThreads] [-w] [-r]");
            Console.WriteLine("\t-i input file list");
            Console.WriteLine("\t-h host name");
            Console.WriteLine("\tLoop Mode Parameters");
            Console.WriteLine("\t\t-s number of milliseconds to sleep between requests");
            Console.WriteLine("\t\t-t number of concurrent threads (max 10)");
            Console.WriteLine("\t\t-w run as web server (listens on port 4122)");
            Console.WriteLine("\t\t-r randomize requests");
        }
    }
}
