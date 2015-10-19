using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    public class ValueParameter : Parameter
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
        public override ushort ToWord()
        {
            return (ushort)(Value << 4);
        }
        #endregion
    }
}
