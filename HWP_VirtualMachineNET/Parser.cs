﻿using HWP_VirtualMachineNET.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET
{
    public class Parser
    {
        public static Instruction[] Parse(string file)
        {
            List<Instruction> instructions = new List<Instruction>();
            Dictionary<string, ushort> labelAddresses = new Dictionary<string, ushort>();

            using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
            {
                Console.WriteLine("> Parsing file \"{0}\"...", file);
                string line = null;
                int lineNum = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNum++;
                    line = line.Trim();
                    if (line.Length == 0) //Ignore empty lines
                        continue;
                    if (line.StartsWith(";")) //comment
                        continue;
                    if (line.Contains(";")) //Cut off comment
                        line = line.Split(';')[0];
                    if (line.Contains(",")) //trim spaces before/after comma
                        line = line.Replace(", ", ",").Replace(" ,", ",");
                    if (line.EndsWith(":")) //label
                    {
                        labelAddresses.Add(line.Substring(0, line.Length - 1), (ushort)(instructions.Count * 2));
                        continue;
                    }
                    line = line.Trim();

                    string[] parts = line.Split(' '); //Split by space: Seperate opcode from parameters
                    parts[0] = parts[0].ToUpper();
                    Instruction.eOpCode opCode = Instruction.eOpCode.NOP;

                    try
                    {
                        opCode = (Instruction.eOpCode)Enum.Parse(typeof(Instruction.eOpCode), parts[0]);
                    }
                    catch
                    {
                        ThrowAsmException(lineNum, "Unknown opcode \"{0}\"", parts[0]);
                    }

                    switch (opCode)
                    {
                        // Those instructions don't require any arguments
                        case Instruction.eOpCode.NOP:
                        case Instruction.eOpCode.RTS:
                        case Instruction.eOpCode.DMP:
                            instructions.Add(new Instruction(opCode));
                            break;
                        //Those ones require one argument
                        case Instruction.eOpCode.LOAD:
                        case Instruction.eOpCode.JSR:
                        case Instruction.eOpCode.PUSH:
                        case Instruction.eOpCode.POP:
                            if (parts.Length != 2)
                                ThrowAsmException(lineNum, "Too few words for \"{0}\"", opCode.ToString());
                            Instruction.Argument[] args0 = ExtractArgs(lineNum, parts[1], 1);
                            instructions.Add(new Instruction(opCode, new ValueParameter(args0[0].Value)));
                            break;
                        case Instruction.eOpCode.JIH:
                        case Instruction.eOpCode.JIZ:
                        case Instruction.eOpCode.JMP:
                            if (parts.Length != 2)
                                ThrowAsmException(lineNum, "Too few words for \"{0}\"", opCode.ToString());
                            instructions.Add(new JMPInstruction(opCode, parts[1]));
                            break;
                        case Instruction.eOpCode.JTS:
                            if (parts.Length != 2)
                                ThrowAsmException(lineNum, "Too few words for \"{0}\"", opCode.ToString());
                            instructions.Add(new JMPInstruction(opCode, parts[1]));
                            break;
                        case Instruction.eOpCode.INC:
                            if (parts.Length != 2)
                                ThrowAsmException(lineNum, "Too few words for \"{0}\"", opCode.ToString());
                            Instruction.Argument[] args2 = ExtractArgs(lineNum, parts[1], 1);
                            byte reg = (byte)args2[0].Value;
                            instructions.Add(new Instruction(Instruction.eOpCode.PUSH, new ValueParameter(0))); //Push r0 to stack
                            instructions.Add(new Instruction(Instruction.eOpCode.LOAD, new ValueParameter(1))); //Load 1 into r0
                            instructions.Add(new Instruction(Instruction.eOpCode.ADD, new RegisterParameter(reg, 0, false, false))); //add r0 to reg
                            instructions.Add(new Instruction(Instruction.eOpCode.POP, new ValueParameter(0))); //Pop r0 back from stack
                            break;
                        case Instruction.eOpCode.DEC:
                            if (parts.Length != 2)
                                ThrowAsmException(lineNum, "Too few words for \"{0}\"", opCode.ToString());
                            Instruction.Argument[] args3 = ExtractArgs(lineNum, parts[1], 1);
                            byte reg1 = (byte)args3[0].Value;
                            //instructions.Add(new Instruction(Instruction.eOpCode.PUSH, new ValueParameter(0))); //Push r0 to stack
                            instructions.Add(new Instruction(Instruction.eOpCode.LOAD, new ValueParameter(1))); //Load 1 into r0
                            instructions.Add(new Instruction(Instruction.eOpCode.SUB, new RegisterParameter(reg1, 0, false, false))); //subtract r0 from reg
                            //instructions.Add(new Instruction(Instruction.eOpCode.POP, new ValueParameter(0))); //Pop r0 back from stack
                            break;
                        //Those ones require two arguments
                        case Instruction.eOpCode.ADD:
                        case Instruction.eOpCode.SUB:
                        case Instruction.eOpCode.DIV:
                        case Instruction.eOpCode.MUL:
                        case Instruction.eOpCode.MOV:
                            if (parts.Length != 2)
                                ThrowAsmException(lineNum, "Too few words for \"{0}\"", opCode.ToString());
                            Instruction.Argument[] args1 = ExtractArgs(lineNum, parts[1], 2);
                            instructions.Add(new Instruction(opCode,
                                new RegisterParameter(
                                    (byte)args1[0].Value, (byte)args1[1].Value, args1[1].MemoryAddress, args1[0].MemoryAddress
                                    )));
                            break;
                        default:
                            ThrowAsmException(lineNum, "Couldn't process OpCode \"{0}\"!", opCode.ToString());
                            break;
                    }
                }
                foreach (Instruction i in instructions)
                {
                    if (i is JMPInstruction)
                        if (!((JMPInstruction)i).Resolve(labelAddresses))
                            ThrowAsmException(0, "Unable to resolve label \"{0}\"!", ((JMPInstruction)i).LabelName);
                }
            }
            Console.WriteLine("> Parsing finished: {0} instructions parsed!", instructions.Count);
            return instructions.ToArray();
        }

        private static Instruction.Argument[] ExtractArgs(int lineNum, string args, int argNum)
        {
            string[] _args = args.Split(',');
            for (int i = 0; i < _args.Length; i++)
                _args[i] = _args[i].Trim().ToUpper();

            if (_args.Length != argNum)
                ThrowAsmException(lineNum, "Expected {0} arguments, found {1}!", argNum.ToString(), _args.Length.ToString());

            Instruction.Argument[] iArgs = new Instruction.Argument[argNum];
            try
            {
                for (int i = 0; i < iArgs.Length; i++)
                    iArgs[i] = new Instruction.Argument(_args[i].Trim());
            }
            catch (Exception ex)
            {
                ThrowAsmException(lineNum, "Failed to extract arguments: {0}", ex.Message);
            }
            return iArgs;
        }

        private static void ThrowAsmException(int line, string message, params string[] args)
        {
            throw new Exception(string.Format("[{0}] {1}", line.ToString().PadLeft(4), string.Format(message, args)));
        }
    }
}