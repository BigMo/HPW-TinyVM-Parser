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
        public static bool SaveBinary { get; set; }
        public static bool SaveBinaryText { get; set; }
        public static bool SaveASM { get; set; }
        public static bool Print { get; set; }
        public static string FileName { get; set; }
        #endregion

        static void Main(string[] args)
        {
            Console.Title = "HWP - VirtualMachineNET Parser";
            ProcessArgs(args);
            do {
                Console.Clear();
                try
                {
                    Instruction[] instructions = Parser.Parse(FileName);
                    if (Print)
                        PrintInstructions(instructions);
                    if (SaveBinary)
                        OutputInstructions(instructions, FileName);
                    if (SaveBinaryText)
                        OutputInstructionsBinary(instructions, FileName);
                    if (SaveASM)
                        OutputASM(instructions, FileName);
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            } while (GetString("Would you like to repeat parsing?", "y", "n").Equals("y"));
            PrintInfo("Done.");
            Console.ReadKey();
        }

        #region File-Methods
        private static void OutputInstructions(Instruction[] instructions, string fileName)
        {
            PrintInfo("> Saving parsed instructions...");
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        foreach (Instruction i in instructions)
                            writer.Write(i.ToWord());
                    }
                    File.WriteAllBytes(fileName.Replace(".asm", ".bin"), stream.ToArray());
                }
                PrintSuccess("> Succesfully saved!");
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        }
        private static void OutputInstructionsBinary(Instruction[] instructions, string fileName)
        {
            PrintInfo("> Saving parsed instructions (binary)...");
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName.Replace(".asm", ".bin2")))
                {
                    foreach (Instruction i in instructions)
                        writer.WriteLine(Convert.ToString(i.ToWord(), 2).PadLeft(16, '0'));
                }

                PrintSuccess("> Succesfully saved!");
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        }
        private static void PrintInstructions(Instruction[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                PrintInfo("Instruction #{0} [@0x{1}]:", (i + 1), (i * sizeof(ushort)).ToString("X").PadLeft(8, '0'));
                PrintInfo("\tOpCode: {0} ({1})", instructions[i].OpCode.ToString(), ((int)instructions[i].OpCode).ToString());
                if (instructions[i].Parameter is ValueParameter)
                {
                    PrintInfo("\tValue: {0}", ((ValueParameter)instructions[i].Parameter).Value.ToString());
                }
                else
                {
                    PrintInfo("\tRX: {0}", ((RegisterParameter)instructions[i].Parameter).DestinationRegister.ToString());
                    PrintInfo("\tRY: {0}", ((RegisterParameter)instructions[i].Parameter).SourceRegister.ToString());
                    PrintInfo("\tToMem: {0}", ((RegisterParameter)instructions[i].Parameter).ToMem.ToString());
                    PrintInfo("\tFromMem: {0}", ((RegisterParameter)instructions[i].Parameter).FromMem.ToString());
                }
            }
        }
        private static void OutputASM(Instruction[] instructions, string fileName)
        {
            PrintInfo("> Saving parsed instructions (binary)...");
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName.Replace(".asm", ".processed.asm")))
                {
                    int idx = 0;
                    foreach (Instruction i in instructions)
                    {
                        writer.Write("{0} ", i.OpCode.ToString());
                        if(i.Parameter is ValueParameter)
                        {
                            writer.Write("0x{0}", ((ValueParameter)i.Parameter).Value.ToString("X").PadLeft(4, '0'));
                        } else
                        {
                            RegisterParameter p = (RegisterParameter)i.Parameter;
                            string rx = string.Format("R{0}", p.DestinationRegister);
                            string ry = string.Format("R{0}", p.SourceRegister);
                            if (p.ToMem)
                                rx = string.Format("[{0}]", rx);
                            if (p.FromMem)
                                ry = string.Format("[{0}]", ry);

                            writer.Write("{0}, {1}", rx, ry);
                        }
                        writer.WriteLine("; {0}", (idx*2).ToString("X").PadLeft(4,'0'));
                        idx++;
                    }
                }
                PrintSuccess("> Succesfully saved!");
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        }
        #endregion
        #region CL Methods
        private static void ProcessArgs(string[] args)
        {
            if (args.Length == 0)
            {
                PrintInfo("* Usage:");
                PrintInfo("* parser.exe <asm-source> [-noBinary] [-saveASM] [-saveBinaryText] [-print]");
                PrintInfo("* \tasm-source: The source-file you'd like to parse and compile to binary");
                PrintInfo("* \t-noBinary: Don't generate binary output");
                PrintInfo("* \t-saveASM: Save the preprocessed ASM to file");
                PrintInfo("* \t-saveBinaryText: Save bit-representation of the compiled instructions to text-file");
                PrintInfo("* \t-print: Prints the compiled instructions");

                FileName = GetFile("Please specify which file you'd like to parse:");
                SaveBinary = !GetString("Do you want to skip compiling to binary?", "y", "n").Equals("y");
                SaveBinaryText = GetString("Do you want to generate bit-representing text-output?", "y", "n").Equals("y");
                SaveASM = GetString("Do you want to save the resulting ASM-instructions?", "y", "n").Equals("y");
                Print = GetString("Do you want to print the resulting ASM-instructions?", "y", "n").Equals("y");
            }
            else
            {
                FileName = args[0];
                SaveASM = FindString(args, "-saveASM");
                SaveBinary = !FindString(args, "-noBinary");
                SaveBinaryText = FindString(args, "-saveBinaryText");
                Print = FindString(args, "-print");
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
