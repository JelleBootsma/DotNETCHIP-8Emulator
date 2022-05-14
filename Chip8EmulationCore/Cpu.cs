namespace Chip8EmulationCore
{
    /// <summary>
    /// Description of the CHIP-8 CPU.
    /// 
    /// 
    /// </summary>
    public class Cpu
    {
        private readonly byte[] _memory = new byte[4096]; // CHIP-8 has 4k of memory
        private readonly byte[] _v = new byte[16]; // 16 8-bit data registers, named V0 to VF
        private readonly ushort _i = 0; // 16-bit address register

        private readonly ushort _pc = 0; // Program counter
        private readonly byte _sp = 0; // Stack pointer


    }
}