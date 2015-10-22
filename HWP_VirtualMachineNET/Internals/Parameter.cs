using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    /// <summary>
    /// A basic ASM-parameter
    /// - has to offer a method to convert its content to a 2-byte value only
    /// </summary>
    public interface IParameter
    {
        ushort ToWord();
    }
}
