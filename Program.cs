﻿using Quelle.DriverDefault;
using Quelle.Listener;
using QuelleHMI;
using QuelleHMI.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace QuelleDriverInterpreter
{
    class Program
    {
        static void prompt()
        {
            Console.WriteLine();
            Console.Write("> ");
        }
        static Dictionary<uint, Thread> Listeners = new Dictionary<uint, Thread>();
        static void Main(string[] args)
        {
            var driver = new QuelleDriver();

            Console.Write("> ");
            for (string line = Console.ReadLine(); /**/; line = Console.ReadLine())
            {
                line = line.Trim();
                if (line.Length < 0)
                {
                    prompt();
                    continue;
                }
                //  Bybass HMIStatement only for @Help, @Exit, @start, and @stop
                //
                if (line.ToLower() == "@exit")
                {
                    break;
                }
                if (line.Trim().ToLower().StartsWith("@help"))
                {
                    if (line.ToLower() == "@help")
                        Console.WriteLine(driver.Help());
                    else
                        Console.WriteLine(driver.Help(line.Substring("@help".Length)));
                    prompt();
                    continue;
                }

                #if ABILITY_TO_START_STOP_SERVER_ON_LOCALHOST
                if (line.ToLower().StartsWith("@start"))
                {
                    var port = uint.Parse(line.Substring("@start".Length));
                    if (port > 0 && !Listeners.ContainsKey(port))
                    {
                        var listener = Listener.Start(port);
                        Listeners.Add(port, listener);
                    }
                    continue;
                }
                if (line.ToLower() == "@stop")
                {
                    Listener.Stop(Listeners);
                    continue;
                }
                #endif

                HMICommand command = new HMICommand(line);

                if (command.statement != null && command.statement.segmentation != null && command.statement.segmentation.Count >= 1 && command.errors.Count == 0)
                {
                    var ok = command.statement.Execute();

                    if (ok)
                    {
                        foreach (var message in command.warnings)
                        {
                            Console.WriteLine("WARNING: " + message);
                        }
                    }
                    else
                    {
                        foreach (var message in command.errors)
                        {
                            Console.WriteLine("ERROR: " + message);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("error: " + "Statement is not expected to be null; Quelle driver implementation error");
                }
                prompt();
            }
            Console.WriteLine("done.");
        }
    }
}
