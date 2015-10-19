using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    public class JMPInstruction : Instruction
    {
        public string LabelName { get; private set; }
        public ushort Address { get; set; }

        public JMPInstruction(eOpCode opCode, string labelName) : base(opCode, new ValueParameter())
        {
            LabelName = labelName;
        }

        public bool Resolve(Dictionary<string, ushort> labelAddresses)
        {
            if (!labelAddresses.ContainsKey(LabelName))
            {
                if (OpCode == eOpCode.JTS) //JTS requires a labelname
                {
                    return false;
                }
                else //Others don't: Labelname could contains an address
                {
                    try
                    {
                        Argument arg = new Argument(LabelName);
                        ((ValueParameter)Parameter).Value = arg.Value;
                    }
                    catch { return false; }
                }
            }
            ((ValueParameter)Parameter).Value = labelAddresses[LabelName];
            return true;
        }
    }
}
