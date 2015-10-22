using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET
{
    /// <summary>
    /// An exception that is thrown while parsing source-code
    /// </summary>
    class SourceCodeException : Exception
    {
        public int LineNumber { get; set; }
        
        public SourceCodeException(int lineNumber, string message, params object[] parameters) 
            : base(string.Format("[{0}] {1}", lineNumber.ToString().PadLeft(4), string.Format(message, parameters)))
        {
            LineNumber = LineNumber;
        }

        public SourceCodeException(int lineNumber, Exception ex)
            : this(lineNumber, ex.Message)
        {

        }
    }
}
