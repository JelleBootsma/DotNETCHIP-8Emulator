using Chip8EmulationCore.IOInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulationCore
{
    public abstract class DisplayBase : IDisplay
    {
        protected readonly byte[] _buffer = new byte[256]; // 64x32 pixels => 8 * 32 bytes => 256 bytes for buffer

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        public bool Draw(byte x, byte y, byte height, ReadOnlySpan<byte> spriteData)
        {
            byte offset = (byte)(x / 8);
            bool setToUnset = false;

            // If the sprite is perfectly aligned with byte endings,
            // only one column of bytes needs to be changed.
            // Otherwise, two columns need to be changed.            
            bool aligned = x % 8 == 0;
            if (aligned)
                for (byte line = 0; line < height; line++)
                {
                    var originalData = _buffer[8 * (y + line) + offset];
                    var newData = (byte)(originalData ^ spriteData[line]);
                    _buffer[8 * (y + line) + offset] = newData;
                    byte changes = (byte)(originalData ^ newData);
                    if ((changes & originalData) != 0x0)
                        setToUnset = true;
                }
            else
                for (byte line = 0; line < height; line++)
                {
                    ushort originalData = 
                        (ushort)(_buffer[8 * (y + line) + offset] << 8 | _buffer[8 * (y + line) + offset + 1]);

                    var shift = 8 - (x % 8);
                    ushort spriteLine = (ushort)(spriteData[line] << shift);
                    var newData = (ushort)(originalData ^ spriteLine);

                    _buffer[8 * (y + line) + offset] = (byte)(newData >> 8);
                    _buffer[8 * (y + line) + offset + 1] = (byte)(newData & 0xFF);
                    
                    ushort changes = (ushort)(originalData ^ newData);
                    if ((changes & originalData) != 0x0)
                        setToUnset = true;
                }
            return setToUnset;
        }
    }
}


