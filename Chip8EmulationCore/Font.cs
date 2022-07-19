using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulationCore
{
    /// <summary>
    /// Class to contain the default chip-8 font.
    /// </summary>
    internal class Font
    {
        /// <summary>
        /// Raw font bytes, index by their character
        /// </summary>
        private static Dictionary<byte, byte[]> FontData { get; } = new Dictionary<byte, byte[]>()
        {
            {0x0,
            new byte[] {
                0b01100000,
                0b10010000,
                0b10010000,
                0b10010000,
                0b01100000
            } },
            {0x1, new byte[] {
                0b00100000,
                0b01100000,
                0b00100000,
                0b00100000,
                0b01110000
            } },
            {0x2, new byte[] {
                0b01100000,
                0b10010000,
                0b00100000,
                0b01000000,
                0b11110000
            } },
            {0x3, new byte[] {
                0b01100000,
                0b10010000,
                0b00100000,
                0b10010000,
                0b01100000
            } },
            {0x4, new byte[] {
                0b00010000,
                0b00110000,
                0b01010000,
                0b11110000,
                0b00010000
            } },
            {0x5, new byte[] {
                0b11110000,
                0b10000000,
                0b11100000,
                0b00010000,
                0b11100000
            } },
            {0x6, new byte[] {
                0b00010000,
                0b00100000,
                0b01100000,
                0b10010000,
                0b01100000
            } },
            {0x7, new byte[] {
                0b11110000,
                0b00100000,
                0b01000000,
                0b01000000,
                0b10000000
            } },
            {0x8, new byte[] {
                0b01100000,
                0b10010000,
                0b01100000,
                0b10010000,
                0b01100000
            } },
            {0x9, new byte[] {
                0b01100000,
                0b10010000,
                0b01100000,
                0b01000000,
                0b10000000
            } },
            {0xA, new byte[] {
                0b00000000,
                0b01100000,
                0b10010000,
                0b11110000,
                0b10010000
            } },
            {0xB, new byte[] {
                0b11100000,
                0b10010000,
                0b11100000,
                0b10010000,
                0b11100000
            } },
            {0xC, new byte[] {
                0b01100000,
                0b10010000,
                0b10000000,
                0b10010000,
                0b01100000
            } },
            {0xD, new byte[] {
                0b11100000,
                0b10110000,
                0b10010000,
                0b10110000,
                0b11100000
            } },
            {0xE, new byte[] {
                0b1111000,
                0b10000000,
                0b11100000,
                0b10000000,
                0b11110000
            } },
            {0xF, new byte[] {
                0b11110000,
                0b10000000,
                0b11100000,
                0b10000000,
                0b10000000
            } }
        };

        /// <summary>
        /// Load the default font into memory.
        /// 
        /// Memory write will start the location <code>currentOffset</code>, and
        /// in total 16 * 5 = 72 bytes will be written to the memory.
        /// Any data already present in those 72 bytes will be overwritten
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="currentOffset">Address to start writing. Will be incremented to end right after last font data</param>
        /// <returns>Dictionary of memory addresses for each character</returns>
        public static Dictionary<byte, ushort> LoadFontIntoMemory(Span<byte> memory, ref ushort currentOffset)
        {
            var result = new Dictionary<byte, ushort>();
            foreach (var character in FontData.Keys)
            {
                result[character] = currentOffset;
                for (var i = 0; i < 5; i++)
                    memory[currentOffset + i] = FontData[character][i];
                currentOffset += 5;
            }
            return result;
        }
    }
}
