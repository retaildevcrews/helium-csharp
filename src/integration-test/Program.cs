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
        static readonly string defaultInputFile = "integration-test.json";
        static readonly string fileNotFoundError = "File not found: {0}";
        static readonly string sleepParameterError = "Invalid sleep (millisecond) parameter: {0}\r\n";
        static readonly string threadsParameterError = "Invalid number of concurrent threads parameter: {0}\r\n";
        public static readonly Config config = new Config();
        public static readonly List<TaskRunner> taskRunners = new List<TaskRunner>();
        public static Smoker.Test smoker;

        public static void Main(string[] args)
        {
            ProcessEnvironmentVariables();

            ProcessCommandArgs(args);

            ValidateParameters();

            smoker = new Smoker.Test(config.FileList, config.Host);

            // run one test iteration
            if (!config.RunLoop && !config.RunWeb)
            {
                if (!smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            IWebHost host = null;

            // setup ctl c handler
            bool cancel = false;

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                Console.WriteLine("Ctl-C Pressed - Starting shutdown ...");
                e.Cancel = true;
                cancel = true;
            };

            // run as a web server
            if (config.RunWeb)
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
            if (config.RunLoop)
            {
                TaskRunner tr;

                for (int i = 0; i < config.Threads; i++)
                {
                    tr = new TaskRunner();
                    tr.TokenSource = new CancellationTokenSource();
                    tr.Task = smoker.RunLoop(i, App.config, tr.TokenSource.Token);

                    taskRunners.Add(tr);
                }
            }

            if (config.RunWeb)
            {
                while (!cancel)
                {
                    try
                    {
                        Console.WriteLine("Version: {0}", Helium.Version.AssemblyVersion);

                        host.Run();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Web Server Exception\n{0}", ex);
                    }
                }

                Console.WriteLine("Web server shutdown");

                return;
            }
            
            if (config.RunLoop && taskRunners.Count > 0)
            {
                // Wait for all tasks to complete
                List<Task> tasks = new List<Task>();

                foreach (var trun in taskRunners)
                {
                    tasks.Add(trun.Task);
                }

                // wait for the run loop to complete or ctrl c
                Task.WaitAll(tasks.ToArray());

                Console.WriteLine("tasks Completed");
            }
        }

        private static void ValidateParameters()
        {
            // make it easier to pass host
            if (!config.Host.ToLower().StartsWith("http"))
            {
                if (config.Host.ToLower().StartsWith("localhost"))
                {
                    config.Host = "http://" + config.Host;
                }
                else
                {
                    config.Host = string.Format("https://{0}.azurewebsites.net", config.Host);
                }
            }

            if (config.SleepMs < 0)
            {
                config.SleepMs = 0;
            }

            if (config.Threads > 0)
            {
                // set config.RunLoop to true
                config.RunLoop = true;
            }

            if (config.Threads < 0)
            {
                config.Threads = 0;
            }

            // let's not get too crazy
            if (config.Threads > 10)
            {
                config.Threads = 10;
            }

            // add default files
            if (config.FileList.Count == 0)
            {
                config.FileList.Add(defaultInputFile);
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
                        Console.WriteLine("\nInvalid argument: {0}\n", args[i]);
                        Usage();
                        Environment.Exit(-1);
                    }

                    // handle web (-w)
                    if (args[i] == "-w")
                    {
                        config.RunWeb = true;
                    }

                    else if (args[i] == "-r")
                    {
                        config.Random = true;
                    }

                    else if (args[i] == "-v")
                    {
                        config.Verbose = true;
                    }

                    // process all other args in pairs
                    else if (i < args.Length - 1)
                    {
                        // handle host (-h http://localhost:4120/)
                        if (args[i] == "-h")
                        {
                            config.Host = args[i + 1].Trim();
                            i++;
                        }

                        // handle input files (-i inputFile.json input2.json input3.json)
                        else if (i < args.Length - 1 && (args[i] == "-i"))
                        {
                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                string file = args[i + 1].Trim();

                                if (System.IO.File.Exists(file))
                                {
                                    config.FileList.Add(file);
                                }
                                else
                                {
                                    Console.WriteLine(fileNotFoundError, file);
                                }

                                i++;
                            }

                            if (config.FileList.Count == 0)
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
                            if (int.TryParse(args[i + 1], out config.SleepMs))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine(sleepParameterError, args[i + 1]);
                                Usage();
                                Environment.Exit(-1);
                            }
                        }

                        // handle config.Threads (-t config.Threads)
                        else if (args[i] == "-t")
                        {
                            if (int.TryParse(args[i + 1], out config.Threads))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine(threadsParameterError, args[i + 1]);
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
                bool.TryParse(env, out config.RunWeb);
            }

            env = Environment.GetEnvironmentVariable("RANDOM");
            if (!string.IsNullOrEmpty(env))
            {
                bool.TryParse(env, out config.Random);
            }

            env = Environment.GetEnvironmentVariable("VERBOSE");
            if (!string.IsNullOrEmpty(env))
            {
                bool.TryParse(env, out config.Verbose);
            }

            env = Environment.GetEnvironmentVariable("HOST");
            if (!string.IsNullOrEmpty(env))
            {
                config.Host = env;
            }

            env = Environment.GetEnvironmentVariable("FILES");
            if (!string.IsNullOrEmpty(env))
            {
                string[] files = env.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string f in files)
                {
                    string file = f.Trim('\'', '\"', ' ');

                    if (System.IO.File.Exists(file))
                    {
                        config.FileList.Add(file);
                    }
                    else
                    {
                        Console.WriteLine(fileNotFoundError, file);
                    }
                }

                if (config.FileList.Count == 0)
                {
                    // exit if no files found
                    Console.WriteLine("No files found");
                    Environment.Exit(-1);
                }
            }

            env = Environment.GetEnvironmentVariable("SLEEP");
            if (!string.IsNullOrEmpty(env))
            {
                if (!int.TryParse(env, out config.SleepMs))
                {
                    // exit on error
                    Console.WriteLine(sleepParameterError, env);
                    Environment.Exit(-1);
                }
            }

            env = Environment.GetEnvironmentVariable("THREADS");
            if (!string.IsNullOrEmpty(env))
            {
                if (!int.TryParse(env, out config.Threads))
                {
                    // exit on error
                    Console.WriteLine(threadsParameterError, env);
                    Environment.Exit(-1);
                }
            }
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
