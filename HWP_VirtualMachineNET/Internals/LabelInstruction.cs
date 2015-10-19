using System;
using System.Collections.Generic;
using System.Text;

namespace HWP_VirtualMachineNET.Internals
{
    /// <summary>
    /// A simple class that allows instructions that use lables to resolve their final address after the rest of a source-file was parsed
    /// </summary>
    public class LabelInstruction : Instruction
    {
        /// <summary>
        /// Name of the label that this instruction will try to resolve
        /// </summary>
        public string LabelName { get; private set; }

        /// <summary>
        /// Initializes a new LabelInstruction assigning the given label-name
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="labelName"></param>
        public LabelInstruction(eOpCode opCode, string labelName) : base(opCode, new ValueParameter())
        {
            LabelName = labelName;
        }

        /// <summary>
        /// This method tries to reolve the address it jumps to by finding its label in the given label-address dictionary.
        /// Caution: Labels should not consists of numbers, only since those can be mistaken for actual addresses rather then names
        /// If no address is found that matches the name of the label, we will try to parse an address
        /// </summary>
        /// <param name="labelAddresses">Dictionary of known labels</param>
        /// <returns></returns>
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
            //Label found, assign its address to the value-field of our value-parameter
            ((ValueParameter)Parameter).Value = labelAddresses[LabelName];
            return true;
        }
    }
}
