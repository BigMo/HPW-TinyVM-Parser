using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    public class Instruction
    {
        #region CLASSES/ENUMS/STRUCTS
        public enum eOpCode
        {
            NOP = 0,
            LOAD,
            ADD,
            SUB,
            MUL,
            MOV,
            DIV,
            PUSH,
            POP,
            JMP,
            JIZ,
            JIH,
            JSR,
            RTS,
            DMP,
            BREAK,
            /* Parser-exclusive */
            INC, //INC <reg> ;increase register by one
            DEC, //DEC <reg> ;decrease register by one
            SET, //SET <reg>,<value> ;sets the value of register
            QSET, //QSET <reg>,<value> ;same as SET, doesn't push/pop r0 though ("quick" set)
            VAR //VAR <name>,<reg>; Renames register to name
        }
        public struct Argument
        {
            public string Representation;
            public ushort Value;
            public bool MemoryAddress;

            public Argument(string representation)
            {
                Representation = representation;
                if (Representation.StartsWith("["))
                {
                    MemoryAddress = true;
                    if (!Representation.EndsWith("]"))
                        throw new Exception("Expected \"]\"");
                    Representation = Representation.Substring(1, Representation.Length - 2);
                }
                else
                {
                    MemoryAddress = false;
                }

                if (Representation.StartsWith("R"))
                    Representation = Representation.Substring(1, Representation.Length - 1);
                bool isHex = false;
                if (Representation.StartsWith("0X"))
                {
                    Representation = Representation.Substring(2, Representation.Length - 2);
                    isHex = true;
                }
                else if (Representation.EndsWith("H"))
                {
                    Representation = Representation.Substring(0, Representation.Length - 1);
                    isHex = true;
                }
                bool success = false;
                if (isHex)
                    success = ushort.TryParse(Representation,
                        NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture,
                        out Value);
                else
                    success = ushort.TryParse(Representation,
                    NumberStyles.Integer,
                    CultureInfo.CurrentCulture,
                    out Value);
                if (!success)
                    throw new Exception(string.Format("Invalid value \"{0}\"", this.Representation));
            }
        }
        #endregion

        #region PROPERTIES
        public eOpCode OpCode { get; set; }
        public Parameter Parameter { get; set; }
        #endregion

        #region CONSTRUCTORS
        public Instruction(eOpCode opCode, Parameter parameter)
        {
            OpCode = opCode;
            Parameter = parameter;
        }
        public Instruction(eOpCode opCode) : this (opCode, new ValueParameter())
        { }
        public Instruction() : this(eOpCode.NOP)
        { }
        public Instruction(ushort fromData)
        {
            OpCode = (eOpCode)(fromData & 0xf);
            switch(OpCode)
            {
                case Instruction.eOpCode.BREAK:
                case Instruction.eOpCode.DMP:
                case Instruction.eOpCode.NOP:
                case Instruction.eOpCode.RTS:
                    break;
                case Instruction.eOpCode.JIH:
                case Instruction.eOpCode.JIZ:
                case Instruction.eOpCode.JMP:
                case Instruction.eOpCode.JSR:
                case Instruction.eOpCode.LOAD:
                case Instruction.eOpCode.POP:
                case Instruction.eOpCode.PUSH:
                    Parameter = new ValueParameter((ushort)(fromData >> 4));
                    break;
                case Instruction.eOpCode.ADD:
                case Instruction.eOpCode.DIV:
                case Instruction.eOpCode.MOV:
                case Instruction.eOpCode.MUL:
                case Instruction.eOpCode.SUB:
                case Instruction.eOpCode.VAR:
                    Parameter = new RegisterParameter(
                        (byte)GetBits(fromData, 4, 4),
                        (byte)GetBits(fromData, 4, 8),
                        Convert.ToBoolean(GetBits(fromData, 4, 12)),
                        Convert.ToBoolean(GetBits(fromData, 4, 13)));
                    break;
                default:
                    OpCode = eOpCode.NOP;
                    Parameter = new ValueParameter(0);
                    break;
            }
        }

        private static ushort GetBits(ushort data, int length, int offset)
        {
            int mask = 0;
            for (int i = 0; i < length; i++)
                mask = (mask << 1) + 1;

            return (ushort)((data & (mask << offset)) >> offset);
        }

        public static Instruction Build(eOpCode opCode, ushort value = 0)
        {
            return new Instruction(opCode, new ValueParameter(value));
        }

        public static Instruction Build(eOpCode opCode, byte rx, byte ry, bool fromMem = false, bool toMem = false)
        {
            return new Instruction(opCode, new RegisterParameter(rx, ry, fromMem, toMem));
        }
        #endregion

        #region METHODS
        public ushort ToWord()
        {
            ushort data = (byte)OpCode;
            data = (ushort)(data + Parameter.ToWord());
            return data;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(OpCode.ToString().ToLower());
            if (Parameter is ValueParameter)
            {
                builder.AppendFormat(" 0x{0}", ((ValueParameter)Parameter).Value.ToString("X").PadLeft(4, '0'));
            }
            else if(Parameter is RegisterParameter)
            {
                RegisterParameter p = (RegisterParameter)Parameter;
                if (p.ToMem)
                    builder.AppendFormat(" [r{0}], ", p.DestinationRegister);
                else
                    builder.AppendFormat(" r{0}, ", p.DestinationRegister);
                if (p.FromMem)
                    builder.AppendFormat("[r{0}]", p.SourceRegister);
                else
                    builder.AppendFormat("r{0}", p.SourceRegister);
            }
            return builder.ToString();
        }
        #endregion
    }
}
