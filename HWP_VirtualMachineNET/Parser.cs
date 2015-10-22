using HWP_VirtualMachineNET.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWP_VirtualMachineNET
{
    /// <summary>
    /// Allows parsing of asm source-files and disassembling of compiled binary-files
    /// </summary>
    public class Parser
    {
        #region PROPERTIES
        private static Dictionary<string, ushort> labelAddresses = new Dictionary<string, ushort>();
        private static Dictionary<string, string> variableNames = new Dictionary<string, string>();
        #endregion

        /// <summary>
        /// Attempts to disassemble the given file
        /// </summary>
        /// <param name="file"></param>
        public static void Disassemble(string file)
        {
            Program.PrintInfo("> Disassembling file...");
            List<Instruction> instructions = new List<Instruction>();
            int idx = 0;
            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position <= reader.BaseStream.Length - 2)
                    {
                        try
                        {
                            instructions.Add(new Instruction(reader.ReadUInt16()));
                            idx++;
                        }
                        catch { Program.PrintError("Failed to disassemble instruction at 0x{0}", idx.ToString("X").PadLeft(4, '0')); }
                    }
                }
            }

            if (Program.SmartDisassembly)
            {
                ReplaceDec(instructions);
                ReplaceInc(instructions);
            }

            using (StreamWriter writer = new StreamWriter(file + ".asm"))
                            foreach (Instruction i in instructions)
                                writer.WriteLine(i.ToString());

            Program.PrintSuccess("> Successfully disassembled file!");
        }

        #region Disassembler-Helpers
        private static void ReplaceDec(List<Instruction> instructions)
        {
            int idx = -1;
            while ((idx = FindPattern(instructions, idx+1, Instruction.eOpCode.LOAD, Instruction.eOpCode.SUB)) != -1)
            {
                Instruction[] ins = ExtractInstructions(instructions, idx, 2);
                if (((RegisterParameter)ins[1].Parameter).SourceRegister == 0)
                {
                    instructions.RemoveRange(idx, 2);
                    instructions.Insert(idx, Instruction.Build(Instruction.eOpCode.DEC, ((RegisterParameter)ins[1].Parameter).DestinationRegister));
                }
            }
        }
        private static void ReplaceInc(List<Instruction> instructions)
        {
            int idx = -1;
            while ((idx = FindPattern(instructions, idx+1, Instruction.eOpCode.PUSH, Instruction.eOpCode.LOAD, Instruction.eOpCode.ADD, Instruction.eOpCode.POP)) != -1)
            {
                Instruction[] ins = ExtractInstructions(instructions, idx, 4);
                if (
                    ((ValueParameter)ins[0].Parameter).Value == 0 &&
                    ((ValueParameter)ins[1].Parameter).Value == 1 &&
                    ((RegisterParameter)ins[2].Parameter).SourceRegister == 0 &&
                    ((ValueParameter)ins[3].Parameter).Value == 0
                )
                {
                    instructions.RemoveRange(idx, 4);
                    instructions.Insert(idx, Instruction.Build(Instruction.eOpCode.INC, ((RegisterParameter)ins[2].Parameter).DestinationRegister));
                }
            }
        }
        private static int FindPattern(List<Instruction> instructions, int start, params Instruction.eOpCode[] opCodes)
        {
            for (int i = start; i < instructions.Count - opCodes.Length; i++)
            {
                bool found = true;
                for (int x = 0; x < opCodes.Length; x++)
                {
                    if (instructions[i + x].OpCode != opCodes[x])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;
            }
            return -1;
        }
        private static Instruction[] ExtractInstructions(List<Instruction> instructions, int offset, int length)
        {
            Instruction[] ins = new Instruction[length];
            for (int i = 0; i < length; i++)
                ins[i] = instructions[offset + i];
            return ins;
        }
        #endregion

        /// <summary>
        /// Parses the given file, processes and compiles it
        /// </summary>
        /// <param name="file"></param>
        public static void Parse(string file)
        {
            Program.PrintInfo("> Parsing file...");
            string preProcessed = null;
            using (StreamReader reader = new StreamReader(file, Encoding.ASCII))
                preProcessed = PreProcess(reader);
            string filePP = file.Replace(".asm", ".preprocessed.asm");
            File.WriteAllText(filePP, preProcessed);
            using (StreamReader reader = new StreamReader(filePP, Encoding.ASCII))
            {
                using (FileStream output = new FileStream(file.Replace(".asm", ".bin"), FileMode.Create))
                {
                    Compile(reader, output);
                }
            }
            Program.PrintSuccess("> Successfully parsed file!");

            preProcessed = "";
            using (StreamReader r = new StreamReader(filePP, Encoding.ASCII))
            {
                string line = null;
                int lineNum = 0;
                while ((line = r.ReadLine()) != null)
                {
                    preProcessed += line + "\t; 0x" + (lineNum * 2).ToString("X").PadLeft(4, '0') + "\n";
                    lineNum++;
                }
            }
            File.WriteAllText(filePP, preProcessed);
        }

        /// <summary>
        /// Compiles the given file to binary
        /// </summary>
        /// <param name="inputStream"></param>
        private static void Compile(StreamReader inputStream, FileStream outputStream)
        {
            Program.PrintInfo(" > Compiling file...");
            List<Instruction> instructions = new List<Instruction>();
            string line = null;
            int lineNum = 0;
            while ((line = inputStream.ReadLine()) != null)
            {
                lineNum++;
                Instruction.eOpCode opCode = ExtractOpCode(line);
                switch (opCode)
                {
                    case Instruction.eOpCode.BREAK:
                    case Instruction.eOpCode.DMP:
                    case Instruction.eOpCode.NOP:
                    case Instruction.eOpCode.RTS:
                        instructions.Add(Instruction.Build(opCode));
                        break;
                    case Instruction.eOpCode.LOAD:
                    case Instruction.eOpCode.JIH:
                    case Instruction.eOpCode.JIZ:
                    case Instruction.eOpCode.JMP:
                    case Instruction.eOpCode.JSR:
                    case Instruction.eOpCode.POP:
                    case Instruction.eOpCode.PUSH:
                        try
                        {
                            Instruction.Argument arg = ExtractArgs(GetArgs(line), 1)[0];
                            instructions.Add(Instruction.Build(opCode, arg.Value));
                        }
                        catch (Exception ex) { throw new SourceCodeException(lineNum, ex); }
                        break;
                    case Instruction.eOpCode.DEC:
                    case Instruction.eOpCode.INC:
                    case Instruction.eOpCode.ADD:
                    case Instruction.eOpCode.DIV:
                    case Instruction.eOpCode.MOV:
                    case Instruction.eOpCode.MUL:
                    case Instruction.eOpCode.SUB:
                        try
                        {
                            Instruction.Argument[] args = ExtractArgs(GetArgs(line), 2);
                            instructions.Add(
                                Instruction.Build(opCode, (byte)args[0].Value, (byte)args[1].Value, args[1].MemoryAddress, args[0].MemoryAddress));
                        } catch(Exception ex) { throw new SourceCodeException(lineNum, ex); }
                        break;
                    default:
                        throw new SourceCodeException(lineNum, "Unexpected instruction \"{0}\"", line);
                        break;
                }
            }
            Program.PrintInfo("  > Writing binary output to file...");
            foreach (Instruction i in instructions)
            {
                outputStream.Write(BitConverter.GetBytes(i.ToWord()), 0, sizeof(ushort));
            }
            Program.PrintSuccess(" > Successfully compiled!");
        }

        /// <summary>
        /// Performs simple preprocessing:
        /// Cleans the code, reads and translates labels and variable-names.
        /// Checks for correct syntax
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        private static string PreProcess(StreamReader inputStream)
        {
            labelAddresses.Clear();
            variableNames.Clear();

            Program.PrintInfo(" > Pre-processing file...");

            StringBuilder builder = new StringBuilder();
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
                {
                    Program.PrintInfo("  > Cleaning up code, parsing labels and variables");
                    int instructionCount = 0;
                    string line = null;
                    int lineNum = 0;
                    #region Clean up source, read labels and variable-names
                    while ((line = inputStream.ReadLine()) != null)
                    {
                        lineNum++;

                        #region Basic checks
                        line = line.Trim();
                        if (line.Length == 0) //Ignore empty lines
                            continue;
                        if (line.StartsWith(";")) //Comment
                            continue;
                        if (line.Contains(";")) //Cut off comment
                            line = line.Split(';')[0];
                        if (line.Contains(",")) //Trim spaces before/after comma
                            line = line.Replace(", ", ",").Replace(" ,", ",");
                        line = line.Trim();
                        #endregion
                        #region Labels
                        if (line.EndsWith(":")) //Label
                        {
                            string labelName = line.Substring(0, line.Length - 1);
                            if (labelAddresses.ContainsKey(labelName))
                                throw new SourceCodeException(lineNum, "Redefining label \"{0}\"", labelName);
                            else
                                labelAddresses.Add(labelName, (ushort)(instructionCount * 2));
                            continue;
                        }
                        #endregion
                        #region Replace variable-names
                        line = ReplaceVariableNames(line);
                        #endregion
                        #region Extract opcode
                        Instruction.eOpCode opCode = Instruction.eOpCode.NOP;
                        try { opCode = ExtractOpCode(line); }
                        catch { throw new SourceCodeException(lineNum, "Could not extract opcode of \"{0}\"", line); }
                        #endregion
                        #region Check for proper number of arguments
                        switch (opCode)
                        {
                            case Instruction.eOpCode.BREAK:
                            case Instruction.eOpCode.DMP:
                            case Instruction.eOpCode.NOP:
                            case Instruction.eOpCode.RTS:
                                break;
                            case Instruction.eOpCode.DEC:
                            case Instruction.eOpCode.INC:
                            case Instruction.eOpCode.JIH:
                            case Instruction.eOpCode.JIZ:
                            case Instruction.eOpCode.JMP:
                            case Instruction.eOpCode.JSR:
                            case Instruction.eOpCode.LOAD:
                            case Instruction.eOpCode.POP:
                            case Instruction.eOpCode.PUSH:
                                if (GetNumberOfArgs(line) != 1)
                                    throw new SourceCodeException(lineNum, "{0} requires 1 parameter", opCode);
                                break;
                            case Instruction.eOpCode.ADD:
                            case Instruction.eOpCode.DIV:
                            case Instruction.eOpCode.MOV:
                            case Instruction.eOpCode.MUL:
                            case Instruction.eOpCode.SUB:
                            case Instruction.eOpCode.VAR:
                            case Instruction.eOpCode.SET:
                                if (GetNumberOfArgs(line) != 2)
                                    throw new SourceCodeException(lineNum, "{0} requires 2 parameter", opCode);
                                break;
                        }
                        #endregion
                        #region Process opcode
                        switch (opCode)
                        {
                            case Instruction.eOpCode.VAR:
                                ExtractVariableName(line);
                                break;
                            case Instruction.eOpCode.INC:
                                string arg0 = ExtractArg(line);
                                writer.WriteLine("push r0");
                                writer.WriteLine("load 1");
                                writer.WriteLine("add {0},r0", arg0);
                                writer.WriteLine("pop r0");
                                instructionCount += 4;
                                break;
                            case Instruction.eOpCode.DEC:
                                string arg1 = ExtractArg(line);
                                writer.WriteLine("load 1");
                                writer.WriteLine("sub {0},r0", arg1);
                                instructionCount += 2;
                                break;
                            case Instruction.eOpCode.QSET:
                                string[] args0 = GetArgs(line);
                                writer.WriteLine("load {0}", args0[1]);
                                writer.WriteLine("mov {0},r0", args0[0]);
                                instructionCount += 2;
                                break;
                            case Instruction.eOpCode.SET:
                                string[] args1 = GetArgs(line);
                                writer.WriteLine("push r0");
                                writer.WriteLine("load {0}", args1[1]);
                                writer.WriteLine("mov {0},r0", args1[0]);
                                writer.WriteLine("pop r0");
                                instructionCount += 4;
                                break;
                            default:
                                writer.WriteLine(line);
                                instructionCount++;
                                break;
                        }
                        #endregion
                    }
                    #endregion
                }
                data = stream.ToArray();
            }
            using (MemoryStream source = new MemoryStream(data))
            {
                using (StreamReader refactoredStream = new StreamReader(source, Encoding.ASCII))
                {
                    Program.PrintInfo("  > Translating labels");
                    int lineNum = 0;
                    string line = null;
                    #region Translate all instructions that use label-names
                    while ((line = refactoredStream.ReadLine()) != null)
                    {
                        lineNum++;
                        Instruction.eOpCode opCode = ExtractOpCode(line);
                        switch (opCode)
                        {
                            case Instruction.eOpCode.JIH:
                            case Instruction.eOpCode.JIZ:
                            case Instruction.eOpCode.JMP:
                            case Instruction.eOpCode.JSR:
                                string arg = ExtractArg(line);
                                if (labelAddresses.ContainsKey(arg))
                                    arg = string.Format("0x{0}", labelAddresses[arg].ToString("X").PadLeft(4, '0'));
                                builder.AppendLine(string.Format("{0} {1}", opCode, arg));
                                break;
                            default:
                                builder.AppendLine(line);
                                break;
                        }
                    }
                    #endregion
                }
            }

            Program.PrintSuccess(" > Successfully pre-processed file!");

            return builder.ToString();
        }

        #region Parse-Helpers
        private static int GetNumberOfArgs(string line)
        {
            string arg = ExtractArg(line);
            if (string.IsNullOrEmpty(arg))
                return 0;
            else if (arg.Contains(","))
                return arg.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
            else
                return 1;
        }
        private static string[] GetArgs(string line)
        {
            return line.Split(' ')[1].Split(',');
        }
        private static string ExtractArg(string line)
        {
            if (line.Contains(" "))
                return line.Split(' ')[1];
            else
                return null;
        }
        private static void ExtractVariableName(string line)
        {
            if (GetNumberOfArgs(line) != 2)
                throw new Exception("Invalid number of parameters");
            string[] args = GetArgs(line);
            variableNames.Add(args[0], args[1]);
        }
        private static string ReplaceVariableNames(string line)
        {
            foreach (string key in variableNames.Keys)
                if (line.Contains(key))
                    line = line.Replace(key, variableNames[key]);
            return line;
        }
        private static Instruction.eOpCode ExtractOpCode(string line)
        {
            string[] parts = line.Split(' '); //Split by space: Seperate opcode from parameters
            parts[0] = parts[0].ToUpper();
            return (Instruction.eOpCode)Enum.Parse(typeof(Instruction.eOpCode), parts[0]);
        }
        private static Instruction.Argument[] ExtractArgs(string[] args, int argNum)
        {
            if (args.Length != argNum)
                throw new Exception(string.Format("Expected {0} arguments, got {1}!", argNum.ToString(), args.Length.ToString()));

            for (int i = 0; i < args.Length; i++)
                args[i] = args[i].Trim().ToUpper();

            Instruction.Argument[] iArgs = new Instruction.Argument[argNum];
            for (int i = 0; i < iArgs.Length; i++)
                iArgs[i] = new Instruction.Argument(args[i].Trim());

            return iArgs;
        }
        #endregion
    }
}
