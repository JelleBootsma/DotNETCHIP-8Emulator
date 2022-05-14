using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulationCore
{
    internal class OpParser
    {

        Opcode Parse(ushort rawOp)
        {
            // Only ops where OpId == rawOp are 0x00E0 and 0x00EE
            if (rawOp == 0x00E0 || rawOp == 0x00EE) return new Opcode(rawOp);

            byte p0 = (byte)(rawOp >> 12);
            byte p1 = (byte)((rawOp >> 8) & 0xF);
            byte p2 = (byte)((rawOp >> 4) & 0xF);
            byte p3 = (byte)(rawOp & 0xF);

            // Only 0x0NNN starts with 0 now
            if (p0 == 0x0)
                // We know p0 = 0, so NNN is equal to the rawop
                return new Opcode(0x0, nnn: rawOp);

            // Ops starting with 0x1, 0x2, 0xA and 0xB follow the same format (All have NNN)
            if (p0 == 0x1 ||
                p0 == 0x2 ||
                p0 == 0xA ||
                p0 == 0xB)
                return new Opcode(p0, nnn: (ushort)((p1 << 8) | (p2 << 4) | p3));

            // Ops following format 0x?XNN
            if (p0 == 0x3 ||
                p0 == 0x4 ||
                p0 == 0x6 ||
                p0 == 0x7 ||
                p0 == 0xC)
                return new Opcode(p0, nn: (byte)((p2 << 4) | p3), x: p1);

            // Ops following format 0x?XY?
            // Then OpId becomes 0x00??
            if (p0 == 0x5 ||
                p0 == 0x8 ||
                p0 == 0x9)
                return new Opcode((ushort)((p0 << 4) | p3), x: p1, y: p2);

            // Ops following format 0x?XYN
            if (p0 == 0xD)
                return new Opcode(p0, x: p1, y: p2, n: p3);

            // Ops following format 0x?X??
            // OpId becomes 0x0???
            if (p0 == 0xE ||
                p0 == 0xF)
                return new Opcode((ushort)((p0 << 8) | (p2 << 4) | p3), x: p1);

            throw new InvalidDataException($"opcode 0x{rawOp.ToString("X2")} is not a valid CHIP-8 operation");
        }


    }
}
