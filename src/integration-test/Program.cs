using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    // integration test for testing Helium (or any REST API)
    partial class App
    {
        // main entry point
        public static async Task Main(string[] args)
        {
            int sleepMs = 0;
            string baseUrl = "http://localhost:4120";
            string defaultInputFile = "integration-test.json";
            bool loop = false;
            List<string> fileList = new List<string>();

            // process the command line args
            if (args.Length > 0)
            {
                if (args[0] == "--help")
                {
                    // display usage

                    Usage();
                    return;
                }

                int i = 0;

                while (i < args.Length)
                {
                    // process all of the args in pairs
                    if (i < args.Length - 1)
                    {
                        // handle host (-h http://localhost:4120/)
                        if (args[i] == "-h")
                        {
                            baseUrl = args[i + 1].Trim();

                            // make it easier to pass host
                            if (! baseUrl.ToLower().StartsWith("http"))
                            {
                                if (baseUrl.ToLower().StartsWith("localhost"))
                                {
                                    baseUrl = "http://" + baseUrl;
                                }
                                else
                                {
                                    baseUrl = string.Format("https://{0}.azurewebsites.net", baseUrl);
                                }
                            }

                            i++;
                        }

                        // handle input files (-i inputFile.json input2.json input3.json)
                        else if (i < args.Length - 1 && (args[i] == "-i"))
                        {
                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                string file = args[i + 1].Trim();

                                if (!System.IO.File.Exists(file))
                                {
                                    Console.WriteLine("File not found: {0}", file);
                                    Environment.Exit(-1);
                                }

                                fileList.Add(file);
                                i++;
                            }
                        }

                        // handle sleep (-s sleepMS)
                        else if (args[i] == "-s")
                        {
                            if (int.TryParse(args[i + 1], out sleepMs) && sleepMs > 0)
                            {
                                // set loop to true
                                loop = true;
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine("Invalid sleep (millisecond) paramter: {0}\r\n", args[i + 1]);
                                Usage();
                                return;
                            }
                        }

                        i++;
                    }
                }
            }

            if (fileList.Count == 0)
            {
                fileList.Add(defaultInputFile);
            }

            Smoker.Test smoker = new Smoker.Test(fileList, baseUrl);

            if (! loop)
            {
                if (!smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            await smoker.RunLoop(sleepMs);

        }

        // display the usage text
        private static void Usage()
        {
            Console.WriteLine("Usage: integration-test [-i file1.json [file2.json] [file3.json] ...] [-h hostUrl] [-s sleepMS] [--help]");
        }

    }
}
