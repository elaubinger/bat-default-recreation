using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace bat_default_exe
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;

            var file = new FileInfo(args[0]);

            if (!file.Exists)
                return;

            var stream = file.OpenText();

            var commands = new List<string>();
            while (!stream.EndOfStream)
                commands.Add(stream.ReadLine());
            try
            {
                ExecuteCommands(commands);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Write("Enter any key to exit: ");
                Console.ReadKey();
            }

            #if DEBUG
            Console.ReadKey();
            #endif
        }

        // https://stackoverflow.com/questions/5519328/executing-batch-file-in-c-sharp
        // https://stackoverflow.com/questions/1704791/is-my-process-waiting-for-input
        static void ExecuteCommands(List<string> commands)
        {
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;

            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardError = true;

            process = Process.Start(processInfo);

            var input = process.StandardInput;
            var thread = process.Threads[0];

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine(e.Data);

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (!String.IsNullOrWhiteSpace(e.Data))
                    throw new Exception($"ERROR> {e.Data}");
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            Console.WriteLine("BEGIN OUTPUT");

            foreach (var command in commands)
            {
                input.WriteLine(command);

                /*
                 * WIP functionality to take user input
                var parsed = new BATCommand(command);

                if (parsed.Command == "SET" && parsed.Flags.Contains(@"/P"))
                    input.WriteLine(Console.ReadLine());
                */

            }

            Console.WriteLine("END OUTPUT");

            process.Close();
        }

        private class BATCommand
        {
            public readonly string Command;
            public readonly HashSet<string> Flags = new HashSet<string>();
            public readonly List<string> Parameters = new List<string>();

            public BATCommand(string command)
            {
                command = command.Trim();
                var elems = command.Split(null);
                for(int i = 0; i < elems.Length; i++)
                {
                    var token = elems[i];

                    if (i == 0)
                        Command = token;
                    else if (Regex.IsMatch(token, @"^\s*/"))
                        Flags.Add(token);
                    else Parameters.Add(token);
                }
            }
        }
    }
}
