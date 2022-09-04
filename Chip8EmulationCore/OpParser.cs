namespace Chip8EmulationCore
{
    public static class OpParser
    {

        public static Opcode Parse(ushort rawOp)
        {
            // Only ops where OpId == rawOp are 0x00E0 and 0x00EE
            // This is because these ops do not have any 'variable' component
            if (rawOp == 0x00E0 || rawOp == 0x00EE) return new Opcode(rawOp);

            byte p0 = (byte)(rawOp >> 12);
            byte p1 = (byte)((rawOp >> 8) & 0xF);
            byte p2 = (byte)((rawOp >> 4) & 0xF);
            byte p3 = (byte)(rawOp & 0xF);

            // Only 0x0NNN starts with 0 now
            if (p0 == 0x0)
                // We know p0 = 0, so NNN is equal to the rawop
                return new Opcode(0x0, nnn: rawOp);


            return p0 switch
            {
                // Ops starting with 0x1, 0x2, 0xA and 0xB follow the same format (All have NNN)
                0x1 or 0x2 or 0xA or 0xB => new Opcode(p0, nnn: (ushort)(p1 << 8 | p2 << 4 | p3)),

                // Ops following format 0x?XNN
                0x3 or 0x4 or 0x6 or 0x7 or 0xC => new Opcode(p0, nn: (byte)(p2 << 4 | p3), x: p1),

                // Ops following format 0x?XY?
                // Then OpId becomes 0x00??
                0x5 or 0x8 or 0x9 => new Opcode((ushort)(p0 << 4 | p3), x: p1, y: p2),

                // Ops following format 0x?XYN
                0xD => new Opcode(p0, x: p1, y: p2, n: p3),

                // Ops following format 0x?X??
                // OpId becomes 0x0???
                0xE or 0xF => new Opcode((ushort)(p0 << 8 | p2 << 4 | p3), x: p1),

                _ => throw new InvalidDataException($"opcode 0x{rawOp:X2} is not a valid CHIP-8 operation"),
            };
        }


    }
}
