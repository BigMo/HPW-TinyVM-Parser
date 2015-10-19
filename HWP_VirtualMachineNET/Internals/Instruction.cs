﻿using System;
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
            /* Parser-exclusive */
            JTS, //JTS <label> ;jump to label
            INC, //INC <reg> ;increase register by one
            DEC //DEC <reg> ;decrease register by one
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
                if (Representation.StartsWith("0x"))
                {
                    Representation = Representation.Substring(2, Representation.Length - 2);
                    isHex = true;
                }
                else if (Representation.EndsWith("h"))
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


        #endregion

        #region METHODS
        public ushort ToWord()
        {
            ushort data = (byte)OpCode;
            data = (ushort)(data + Parameter.ToWord());
            return data;
        }
        #endregion
    }
}
