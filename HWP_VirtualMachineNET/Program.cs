using HWP_VirtualMachineNET.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWP_VirtualMachineNET
{
    class Program
    {
        #region PROPERTIES
        public static string FileName { get; set; }
        public static bool Disassemble { get; set; }
        public static bool SmartDisassembly { get; set; }
        #endregion

        static void Main(string[] args)
        {
            Console.Title = "HWP - VirtualMachineNET Parser";
            ProcessArgs(args);
            do {
                Console.Clear();
                try
                {
                    if (!Disassemble)
                        Parser.Parse(FileName);
                    else
                        Parser.Disassemble(FileName);
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            } while (GetString("Would you like to repeat parsing?", "y", "n").Equals("y"));
            PrintInfo("Done.");
            Console.ReadKey();
        }

        #region CL Methods
        private static void ProcessArgs(string[] args)
        {
            FileName = "";
            Disassemble = false;
            SmartDisassembly = false;
            if (args.Length == 0)
            {
                PrintInfo("* Usage:");
                PrintInfo("* parser.exe <asm-source> [-disassemble] [-smartDisassembly]");
                PrintInfo("* \tdisassemble: Try to disassemble the file");
                PrintInfo("* \tsmartDisassembly: Try to replace instructions with more compact ones");

                FileName = GetFile("Please specify which file you'd like to parse:");
                Disassemble = GetString("Would you like to disassemble this file?", "y", "n").Equals("y");
                if (Disassemble)
                    SmartDisassembly = GetString("Would you like to enable smart-disassembly?", "y", "n").Equals("y");
            }
            else
            {
                FileName = args[0];
            }
        }
        private static bool FindString(string[] haystack, string needle)
        {
            foreach (string arg in haystack)
                if (arg.Equals(needle))
                    return true;
            return false;
        }
        #endregion
        #region Utilities
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }
        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }
        public static void PrintException(Exception ex)
        {
            PrintError("An Exception occured: {0}\n\"{1}\"\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
#if DEBUG
            PrintError("StackTrace:\n{0}", ex.StackTrace);
#endif
        }
        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            ConsoleColor clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        private static string GetString(string message, params string[] arrOptions)
        {
            PrintInfo(message);
            if (arrOptions.Length > 0)
            {
                string options = "";
                for (int i = 0; i < arrOptions.Length; i++)
                    options = string.Format("{0}{1}{2}", options, arrOptions[i], i == arrOptions.Length - 1 ? "" : ", ");
                PrintInfo("[Options: {0}]", options);
            }
            string answer = "";
            do {
                Console.Write("> ");
                answer = Console.ReadLine();
            } while (string.IsNullOrEmpty(answer) || (arrOptions.Length > 0 && !FindString(arrOptions, answer)));
            return answer;
        }

        private static string GetFile(string message)
        {
            string file = "";
            do
            {
                file = GetString(message);
            } while (string.IsNullOrEmpty(file) || !File.Exists(file));
            return file;
        }
        #endregion
    }
}
