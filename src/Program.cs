using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public sealed class App
    {
        const string _durationParameterError = "Invalid duration (seconds) parameter: {0}\n";
        const string _fileNotFoundError = "File not found: {0}";
        const string _sleepParameterError = "Invalid sleep (milliseconds) parameter: {0}\n";
        const string _threadsParameterError = "Invalid number of concurrent threads parameter: {0}\n";
        const string _maxAgeParameterError = "Invalid maximum metrics age parameter: {0}\n";

        public static readonly Config Config = new Config();
        public static readonly DateTime StartTime = DateTime.UtcNow;
        public static readonly Metrics Metrics = new Metrics();

        // necessary for private build - do not delete
        public static readonly List<TaskRunner> TaskRunners = new List<TaskRunner>();
        public static Smoker.Test Smoker = null;

        public static void Main(string[] args)
        {
            ProcessEnvironmentVariables();

            ProcessCommandArgs(args);

            ValidateParameters();

            Smoker = new Smoker.Test(Config.FileList, Config.Host);

            // run one test iteration
            if (!Config.RunLoop && !Config.RunWeb)
            {
                if (!Smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            IWebHost host = null;

            // configure web server
            if (Config.RunWeb)
            {
                // use the default web host builder + startup
                IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .UseUrls("http://*:4122/");

                // build the host
                host = builder.Build();
            }

            using CancellationTokenSource ctCancel = new CancellationTokenSource();
            // setup ctl c handler
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

            // run tests in config.RunLoop
            if (Config.RunLoop)
            {
                TaskRunner tr;

                for (int i = 0; i < Config.Threads; i++)
                {
                    tr = new TaskRunner { TokenSource = ctCancel };

                    tr.Task = Smoker.RunLoop(i, App.Config, tr.TokenSource.Token);

                    TaskRunners.Add(tr);
                }
            }

            // run the web server
            if (Config.RunWeb)
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
            if (Config.RunLoop && TaskRunners.Count > 0)
            {
                // Wait for all tasks to complete
                List<Task> tasks = new List<Task>();

                foreach (var trun in TaskRunners)
                {
                    tasks.Add(trun.Task);
                }

                // wait for ctrl c
                Task.WaitAll(tasks.ToArray());
            }
        }

        private static void ValidateParameters()
        {
            // host is required
            if (string.IsNullOrWhiteSpace(Config.Host))
            {
                Console.WriteLine("Must specify --host parameter\n");
                Usage();
                Environment.Exit(-1);
            }

            // invalid parameter
            if (Metrics.MaxAge < 0)
            {
                Console.Write(_maxAgeParameterError, Metrics.MaxAge);
                Usage();
                Environment.Exit(-1);
            }

            // make it easier to pass host
            if (!Config.Host.ToLower().StartsWith("http"))
            {
                if (Config.Host.ToLower().StartsWith("localhost"))
                {
                    Config.Host = "http://" + Config.Host;
                }
                else
                {
                    Config.Host = string.Format($"https://{Config.Host}.azurewebsites.net");
                }
            }

            // validate parameters
            ValidateNonRunloopParameters();
            ValidateRunloopParameters();
            ValidateFileList();
        }

        private static void AddAllJsonFiles()
        {
            string dir = "TestFiles";

            // make sure the directory exists
            if (!Directory.Exists(dir))
            {
                dir = "../../../" + dir;

                if (!Directory.Exists(dir))
                {
                    Console.WriteLine($"No files found in {dir}");
                    Environment.Exit(-1);
                }
            }

            // clear the list
            Config.FileList.Clear();

            // get a list of files
            Config.FileList.AddRange(Directory.GetFiles(dir, "*.json"));
        }

        private static void ValidateFileList()
        {

            if (Config.FileList.Count == 0)
            {
                AddAllJsonFiles();
            }

            if (Config.FileList.Count == 0)
            {
                // exit if no files found
                Console.WriteLine("No files found");
                Environment.Exit(-1);
            }

            string f;

            for (int i = Config.FileList.Count - 1; i >= 0; i--)
            {
                f = Config.FileList[i];

                string file = TestFileExists(f.Trim('\'', '\"', ' '));

                if (System.IO.File.Exists(file))
                {
                    Config.FileList[i] = file;
                }
                else
                {
                    Console.WriteLine(_fileNotFoundError, f);
                    Config.FileList.RemoveAt(i);
                }
            }

            if (Config.FileList.Count == 0)
            {
                // exit if no files found
                Console.WriteLine("No files found");
                Environment.Exit(-1);
            }
        }

        private static void ValidateNonRunloopParameters()
        {
            // validate runloop params / set defaults
            if (!Config.RunLoop)
            {
                // these params require --runloop
                if (Config.RunWeb)
                {
                    Console.WriteLine("Must specify --runloop to use --runweb\n");
                    Usage();
                    Environment.Exit(-1);
                }

                if (Config.Threads != -1)
                {
                    Console.WriteLine("Must specify --runloop to use --threads\n");
                    Usage();
                    Environment.Exit(-1);
                }

                if (Config.SleepMs != -1)
                {
                    Console.WriteLine("Must specify --runloop to use --sleep\n");
                    Usage();
                    Environment.Exit(-1);
                }

                if (Config.Duration > 0)
                {
                    Console.WriteLine("Must specify --runloop to use --duration\n");
                    Usage();
                    Environment.Exit(-1);
                }

                if (Config.Random)
                {
                    Console.WriteLine("Must specify --runloop to use --random\n");
                    Usage();
                    Environment.Exit(-1);
                }
            }

            // invalid combo
            if (Config.RunWeb && Config.Duration > 0)
            {
                Console.WriteLine("Cannot use --duration with --runweb\n");
                Usage();
                Environment.Exit(-1);
            }

            // limit metrics to 12 hours as it's stored in memory
            if (Metrics.MaxAge > 12 * 60 * 60)
            {
                Metrics.MaxAge = 12 * 60 * 60;
            }

            if (Config.Duration < 0)
            {
                Console.WriteLine(_durationParameterError, Config.Duration);
                Usage();
                Environment.Exit(-1);
            }
        }

        private static void ValidateRunloopParameters()
        {
            // validate runloop params / set defaults
            if (Config.RunLoop)
            {
                if (Config.SleepMs == -1)
                {
                    Config.SleepMs = 1000;
                }

                if (Config.Threads == -1)
                {
                    Config.Threads = 1;
                }

                // let's not get too crazy
                if (Config.Threads > 10)
                {
                    Config.Threads = 10;
                }

                if (Config.Threads <= 0)
                {
                    Console.WriteLine(_threadsParameterError, Config.Threads);
                    Usage();
                    Environment.Exit(-1);
                }

                if (Config.SleepMs < 0)
                {
                    Console.WriteLine(_sleepParameterError, Config.SleepMs);
                    Usage();
                    Environment.Exit(-1);
                }
            }
        }

        private static void ProcessCommandArgs(string[] args)
        {
            if (args.Length > 0)
            {
                // display help
                if (args[0].ToLower() == "--help" || args[0].ToLower() == "-h")
                {
                    Usage();
                    Environment.Exit(0);
                }

                int i = 0;

                while (i < args.Length)
                {
                    if (!args[i].StartsWith("--"))
                    {
                        Console.WriteLine($"\nInvalid argument: {args[i]}\n");
                        Usage();
                        Environment.Exit(-1);
                    }

                    // handle run loop (---runloop)
                    if (args[i].ToLower() == "--runloop")
                    {
                        Config.RunLoop = true;
                    }

                    // handle run web (---runweb)
                    else if (args[i].ToLower() == "--runweb")
                    {
                        Config.RunWeb = true;
                    }

                    // handle --random
                    else if (args[i].ToLower() == "--random")
                    {
                        Config.Random = true;
                    }

                    // handle --verbose
                    else if (args[i].ToLower() == "--verbose")
                    {
                        Config.Verbose = true;
                    }

                    // process all other args in pairs
                    else if (i < args.Length - 1)
                    {
                        // handle host
                        if (args[i].ToLower() == "--host")
                        {
                            Config.Host = args[i + 1].Trim();
                            i++;
                        }

                        // handle input files (-i inputFile.json input2.json input3.json)
                        else if (i < args.Length - 1 && (args[i].ToLower() == "--files"))
                        {
                            // command line overrides env var
                            Config.FileList.Clear();

                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                if (!string.IsNullOrEmpty(args[i + 1]))
                                {
                                    Config.FileList.Add(args[i + 1].Trim());
                                }

                                i++;
                            }
                        }

                        // handle sleep (--sleep config.SleepMs)
                        else if (args[i].ToLower() == "--sleep")
                        {
                            if (int.TryParse(args[i + 1], out int v))
                            {
                                Config.SleepMs = v;
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

                        // handle config.Threads (--threads config.Threads)
                        else if (args[i].ToLower() == "--threads")
                        {
                            if (int.TryParse(args[i + 1], out int v))
                            {
                                Config.Threads = v;
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
                        // handle duration (--maxage Metrics.MaxAge (minutes))
                        else if (args[i] == "--maxage")
                        {
                            if (int.TryParse(args[i + 1], out Metrics.MaxAge))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine(_maxAgeParameterError, args[i + 1]);
                                Usage();
                                Environment.Exit(-1);
                            }
                        }

                        // handle duration (--duration config.Duration (seconds))
                        else if (args[i].ToLower() == "--duration")
                        {
                            if (int.TryParse(args[i + 1], out int v))
                            {
                                Config.Duration = v;
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine(_durationParameterError, args[i + 1]);
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

            string env = Environment.GetEnvironmentVariable("RUNLOOP");
            if (!string.IsNullOrEmpty(env))
            {
                if (bool.TryParse(env, out bool b))
                {
                    Config.RunLoop = b;
                }
            }

            env = Environment.GetEnvironmentVariable("RUNWEB");
            if (!string.IsNullOrEmpty(env))
            {
                if (bool.TryParse(env, out bool b))
                {
                    Config.RunWeb = b;
                }
            }

            env = Environment.GetEnvironmentVariable("RANDOM");
            if (!string.IsNullOrEmpty(env))
            {
                if (bool.TryParse(env, out bool b))
                {
                    Config.Random = b;
                }
            }

            env = Environment.GetEnvironmentVariable("VERBOSE");
            if (!string.IsNullOrEmpty(env))
            {
                if (bool.TryParse(env, out bool b))
                {
                    Config.Verbose = b;
                }
            }

            env = Environment.GetEnvironmentVariable("HOST");
            if (!string.IsNullOrEmpty(env))
            {
                Config.Host = env;
            }

            env = Environment.GetEnvironmentVariable("FILES");
            if (!string.IsNullOrEmpty(env))
            {
                Config.FileList.AddRange(env.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            env = Environment.GetEnvironmentVariable("SLEEP");
            if (!string.IsNullOrEmpty(env))
            {
                if (int.TryParse(env, out int v))
                {
                    Config.SleepMs = v;
                }
                else
                {
                    // exit on error
                    Console.WriteLine(_sleepParameterError, env);
                    Environment.Exit(-1);
                }
            }

            env = Environment.GetEnvironmentVariable("THREADS");
            if (!string.IsNullOrEmpty(env))
            {
                if (int.TryParse(env, out int v))
                {
                    Config.Threads = v;
                }
                else
                {
                    // exit on error
                    Console.WriteLine(_threadsParameterError, env);
                    Environment.Exit(-1);
                }
            }

            env = Environment.GetEnvironmentVariable("MAXMETRICSAGE");
            if (!string.IsNullOrEmpty(env))
            {
                if (!int.TryParse(env, out Metrics.MaxAge))
                {
                    // exit on error
                    Console.WriteLine(_maxAgeParameterError, env);
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
            Console.WriteLine($"Version: {Helium.Version.AssemblyVersion}");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run -- [-h] [--help] --host hostUrl [--files file1.json [file2.json] [file3.json] ...]\n[--runloop] [--sleep sleepMs] [--threads numberOfThreads] [--duration durationSeconds] [--random]\n[--runweb] [--verbose] [--maxage maxMinutes]");
            Console.WriteLine("\t--host host name or host Url");
            Console.WriteLine("\t--files file1 [file2 file3 ...] (default *.json)");
            Console.WriteLine("\t--runloop");
            Console.WriteLine("\tLoop Mode Parameters");
            Console.WriteLine("\t\t--sleep number of milliseconds to sleep between requests (default 1000)");
            Console.WriteLine("\t\t--threads number of concurrent threads (default 1) (max 10)");
            Console.WriteLine("\t\t--duration duration in seconds (default forever");
            Console.WriteLine("\t\t--random randomize requests");
            Console.WriteLine("\t\t--runweb run as web server (listens on port 4122)");
            Console.WriteLine("\t\t--verbose turn on verbose logging");
            Console.WriteLine("\t\t--maxage maximum minutes to track metrics (default 240)");
            Console.WriteLine("\t\t\t0 = do not track metrics");
            Console.WriteLine("\t\t\trequires --runweb");
        }
    }
}
