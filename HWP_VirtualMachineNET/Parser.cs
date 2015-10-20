using HWP_VirtualMachineNET.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWP_VirtualMachineNET
{
    public class Parser
    {
        #region PROPERTIES
        private static Dictionary<string, ushort> labelAddresses = new Dictionary<string, ushort>();
        private static Dictionary<string, string> variableNames = new Dictionary<string, string>();
        #endregion

        //public static Instruction[] Parse(string file)
        //{
        //    List<Instruction> instructions = new List<Instruction>();
        //    Dictionary<string, ushort> labelAddresses = new Dictionary<string, ushort>();
        //    Dictionary<string, string> variableNames = new Dictionary<string, string>();

        //    using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
        //    {
        //        Program.PrintInfo("> Parsing file \"{0}\"...", file);
        //        string line = null;
        //        int lineNum = 0;
        //        while ((line = reader.ReadLine()) != null)
        //        {
        //            lineNum++;
        //            #region Basic checks
        //            line = line.Trim();
        //            if (line.Length == 0) //Ignore empty lines
        //                continue;
        //            if (line.StartsWith(";")) //Comment
        //                continue;
        //            if (line.Contains(";")) //Cut off comment
        //                line = line.Split(';')[0];
        //            if (line.Contains(",")) //Trim spaces before/after comma
        //                line = line.Replace(", ", ",").Replace(" ,", ",");
        //            if (line.EndsWith(":")) //Label
        //            {
        //                labelAddresses.Add(line.Substring(0, line.Length - 1), (ushort)(instructions.Count * 2));
        //                continue;
        //            }
        //            line = line.Trim();
        //            #endregion
        //            #region Extract opcode
        //            string[] parts = line.Split(' '); //Split by space: Seperate opcode from parameters
        //            parts[0] = parts[0].ToUpper();
        //            Instruction.eOpCode opCode = Instruction.eOpCode.NOP;

        //            try
        //            {
        //                opCode = (Instruction.eOpCode)Enum.Parse(typeof(Instruction.eOpCode), parts[0]);
        //            }
        //            catch
        //            {
        //                ThrowAsmException(lineNum, "Unknown opcode \"{0}\"", parts[0]);
        //            }
        //            #endregion
        //            #region Check arguments
        //            bool tooFewArgs = false;
        //            switch (opCode)
        //            {
        //                // Those instructions don't require any arguments
        //                case Instruction.eOpCode.NOP:
        //                case Instruction.eOpCode.RTS:
        //                case Instruction.eOpCode.DMP:
        //                    break;
        //                //Those ones require one argument
        //                case Instruction.eOpCode.LOAD:
        //                case Instruction.eOpCode.PUSH:
        //                case Instruction.eOpCode.POP:
        //                case Instruction.eOpCode.JIH:
        //                case Instruction.eOpCode.JIZ:
        //                case Instruction.eOpCode.JMP:
        //                case Instruction.eOpCode.JSR:
        //                case Instruction.eOpCode.DEC:
        //                case Instruction.eOpCode.INC:
        //                case Instruction.eOpCode.ADD:
        //                case Instruction.eOpCode.SUB:
        //                case Instruction.eOpCode.DIV:
        //                case Instruction.eOpCode.MUL:
        //                case Instruction.eOpCode.MOV:
        //                case Instruction.eOpCode.VAR:
        //                    if (parts.Length != 2)
        //                        tooFewArgs = true;
        //                    break;
        //            }
        //            if (tooFewArgs)
        //                ThrowAsmException(lineNum, "Too few arguments for OpCode \"{0}\"", opCode.ToString());
        //            #endregion
        //            #region Process parameters
        //            switch (opCode)
        //            {
        //                // Those instructions don't require any arguments
        //                case Instruction.eOpCode.NOP:
        //                case Instruction.eOpCode.RTS:
        //                case Instruction.eOpCode.DMP:
        //                case Instruction.eOpCode.BREAK:
        //                    instructions.Add(Instruction.Build(opCode));
        //                    break;
        //                //Those ones require one argument
        //                case Instruction.eOpCode.LOAD:
        //                case Instruction.eOpCode.PUSH:
        //                case Instruction.eOpCode.POP:
        //                    Instruction.Argument[] args0 = ExtractArgs(lineNum, parts[1], 1);
        //                    instructions.Add(Instruction.Build(opCode, args0[0].Value));
        //                    break;
        //                case Instruction.eOpCode.VAR:
        //                    string[] vr = parts[1].Split('=');
        //                    if (vr.Length != 2)
        //                        ThrowAsmException(lineNum, "Invalid VAR-instruction \"{0}\"", parts[1]);
        //                    variableNames.Add(vr[0], vr[1]);
        //                    break;
        //                case Instruction.eOpCode.JIH:
        //                case Instruction.eOpCode.JIZ:
        //                case Instruction.eOpCode.JMP:
        //                case Instruction.eOpCode.JSR:
        //                    instructions.Add(new LabelInstruction(opCode, parts[1]));
        //                    break;
        //                case Instruction.eOpCode.INC:
        //                    Instruction.Argument[] args2 = ExtractArgs(lineNum, parts[1], 1);
        //                    byte reg = (byte)args2[0].Value;
        //                    instructions.Add(Instruction.Build(Instruction.eOpCode.PUSH, 0)); //Push r0 to stack
        //                    instructions.Add(Instruction.Build(Instruction.eOpCode.LOAD, 1)); //Load 1 into r0
        //                    instructions.Add(Instruction.Build(Instruction.eOpCode.ADD, reg, 0)); //add r0 to reg
        //                    instructions.Add(Instruction.Build(Instruction.eOpCode.POP, 0)); //Pop r0 back from stack
        //                    break;
        //                case Instruction.eOpCode.DEC:
        //                    Instruction.Argument[] args3 = ExtractArgs(lineNum, parts[1], 1);
        //                    byte reg1 = (byte)args3[0].Value;
        //                    instructions.Add(Instruction.Build(Instruction.eOpCode.LOAD, 1)); //Load 1 into r0
        //                    instructions.Add(Instruction.Build(Instruction.eOpCode.SUB, reg1, 0)); //subtract r0 from reg
        //                    break;
        //                //Those ones require two arguments
        //                case Instruction.eOpCode.ADD:
        //                case Instruction.eOpCode.SUB:
        //                case Instruction.eOpCode.DIV:
        //                case Instruction.eOpCode.MUL:
        //                case Instruction.eOpCode.MOV:
        //                    Instruction.Argument[] args1 = ExtractArgs(lineNum, parts[1], 2);
        //                    instructions.Add(Instruction.Build(opCode, (byte)args1[0].Value, (byte)args1[1].Value, args1[1].MemoryAddress, args1[0].MemoryAddress));
        //                    break;
        //                default:
        //                    ThrowAsmException(lineNum, "Couldn't process OpCode \"{0}\"!", opCode.ToString());
        //                    break;
        //            }
        //            #endregion
        //        }
        //        #region Resolve labels
        //        foreach (Instruction i in instructions)
        //        {
        //            if (i is LabelInstruction)
        //                if (!((LabelInstruction)i).Resolve(labelAddresses))
        //                    ThrowAsmException(0, "Unable to resolve label \"{0}\"!", ((LabelInstruction)i).LabelName);
        //        }
        //        #endregion
        //    }
        //    Program.PrintSuccess("> Parsing finished: {0} instructions parsed!", instructions.Count);
        //    return instructions.ToArray();
        //}

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
        private static void ThrowAsmException(int line, string message, params string[] args)
        {
            throw new Exception(string.Format("[{0}] {1}", line.ToString().PadLeft(4), string.Format(message, args)));
        }
    }
}
