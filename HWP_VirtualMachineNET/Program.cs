using HWP_VirtualMachineNET.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWP_VirtualMachineNET
{
    class Program
    {
        static void Main(string[] args)
        {
            bool print = true;
            bool save = true;
            bool saveBinary = true;
            bool saveASM = true;
            string fileName = "testprog2.asm";
            try
            {
                Instruction[] instructions = Parser.Parse(fileName);
                if (print)
                    PrintInstructions(instructions);
                if (save)
                    SaveInstructions(instructions, fileName);
                if (saveBinary)
                    SaveInstructionsBinary(instructions, fileName);
                if (saveASM)
                    SaveASM(instructions, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse text:\n{0}", ex.Message);
            }
            Console.ReadKey();
        }

        private static void SaveInstructions(Instruction[] instructions, string fileName)
        {
            Console.WriteLine("> Saving parsed instructions...");
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
                Console.WriteLine("> Succesfully saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("> Couldn't save: {0}\n{1}", ex.GetType().Name, ex.Message);
            }
        }

        private static void SaveInstructionsBinary(Instruction[] instructions, string fileName)
        {
            Console.WriteLine("> Saving parsed instructions (binary)...");
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName.Replace(".asm", ".bin2")))
                {
                    foreach (Instruction i in instructions)
                        writer.WriteLine(Convert.ToString(i.ToWord(), 2).PadLeft(16, '0'));
                }

                Console.WriteLine("> Succesfully saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("> Couldn't save: {0}\n{1}", ex.GetType().Name, ex.Message);
            }
        }

        private static void PrintInstructions(Instruction[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                Console.WriteLine("Instruction #{0} [@0x{1}]:", (i + 1), (i * sizeof(ushort)).ToString("X").PadLeft(8, '0'));
                Console.WriteLine("\tOpCode: {0} ({1})", instructions[i].OpCode.ToString(), ((int)instructions[i].OpCode).ToString());
                if (instructions[i].Parameter is ValueParameter)
                {
                    Console.WriteLine("\tValue: {0}", ((ValueParameter)instructions[i].Parameter).Value.ToString());
                }
                else
                {
                    Console.WriteLine("\tRX: {0}", ((RegisterParameter)instructions[i].Parameter).DestinationRegister.ToString());
                    Console.WriteLine("\tRY: {0}", ((RegisterParameter)instructions[i].Parameter).SourceRegister.ToString());
                    Console.WriteLine("\tToMem: {0}", ((RegisterParameter)instructions[i].Parameter).ToMem.ToString());
                    Console.WriteLine("\tFromMem: {0}", ((RegisterParameter)instructions[i].Parameter).FromMem.ToString());
                }
            }
        }

        private static void SaveASM(Instruction[] instructions, string fileName)
        {
            Console.WriteLine("> Saving parsed instructions (binary)...");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("> Couldn't save: {0}\n{1}", ex.GetType().Name, ex.Message);
            }
        }
    }
}
