using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    /// <summary>
    /// A parameter-implementation that holds a 12-bit value
    /// </summary>
    public class ValueParameter : IParameter
    {
        #region PROPERTIES
        public ushort Value { get; set; }
        #endregion

        #region CONSTRUCTOR
        public ValueParameter(ushort value)
        {
            this.Value = value;
        }

        public ValueParameter() : this(0) { }
        #endregion

        #region METHODS
        public ushort ToWord()
        {
            return (ushort)(Value << 4);
        }
        #endregion
    }
}
