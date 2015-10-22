using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    /// <summary>
    /// A parameter-implementation that holds data about register- and memory-access
    /// </summary>
    public class RegisterParameter : IParameter
    {
        #region PROPERTIES
        public byte DestinationRegister { get; set; }
        public byte SourceRegister { get; set; }
        public bool FromMem { get; set; }
        public bool ToMem { get; set; }
        #endregion

        #region CONSTRUCTORS
        public RegisterParameter(byte rdest, byte rsource, bool fromMem, bool toMem)
        {
            DestinationRegister = rdest;
            SourceRegister = rsource;
            FromMem = fromMem;
            ToMem = toMem;
        }
        #endregion

        #region METHODS
        public ushort ToWord()
        {
            ushort data = 0;
            data |= DestinationRegister;
            data |= (ushort)(SourceRegister << 4);
            if (FromMem)
                data |= (ushort)(1 << 8);
            if (ToMem)
                data |= (ushort)(1 << 9);

            data <<= 4;
            return data;
        }
        #endregion
    }
}
