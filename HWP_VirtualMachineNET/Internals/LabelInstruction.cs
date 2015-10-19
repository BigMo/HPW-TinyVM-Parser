using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    public class LabelInstruction : Instruction
    {
        public string LabelName { get; private set; }
        public ushort Address { get; set; }

        public LabelInstruction(eOpCode opCode, string labelName) : base(opCode, new ValueParameter())
        {
            LabelName = labelName;
        }

        public bool Resolve(Dictionary<string, ushort> labelAddresses)
        {
            if (!labelAddresses.ContainsKey(LabelName))
            {
                try
                {
                    Argument arg = new Argument(LabelName); //Try to parse an argument from the label-name; May fail
                    ((ValueParameter)Parameter).Value = arg.Value;
                    return true;
                }
                catch { return false; }
            }
            ((ValueParameter)Parameter).Value = labelAddresses[LabelName];
            return true;
        }
    }
}
