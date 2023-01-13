namespace A2
{
    public class Instruction
    {
        public InstructionType? instructionType { get; set; }
        public uint currentPC { get; set; }
        public uint targetAddress { get; set; }
    }

    public enum InstructionType
    {
        Branch = 0,
        Load,
        Store,
        Arithmetic
    }
}
