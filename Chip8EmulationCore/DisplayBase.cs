using Chip8EmulationCore.IOInterfaces;

namespace Chip8EmulationCore
{
    public abstract class DisplayBase : IDisplay
    {
        public const int Width = 64;
        public const int Height = 32;
        protected readonly byte[] _buffer = new byte[256]; // 64x32 pixels => 8 * 32 bytes => 256 bytes for buffer

        /// <summary>
        /// This method is called after the memory in the basedisplay is changed.
        /// 
        /// Needs to be implemented, so derived classes know when to read the memory 
        /// Included is a byte span of the same shape as the buffer, which indicates which bufferlocations have been changed.
        /// Unchanged memory locations will be 0, where as changed locations will be 1
        /// </summary>
        protected abstract void OnBufferChanged(ReadOnlySpan<byte> changedBytes);

        public void Clear()
        {
            // Bytes that are changed, are the pixels that are originally on. We create a quick copy of the array
            var changeLocations = new byte[256];
            _buffer.CopyTo(changeLocations, 0);
            Array.Clear(_buffer, 0, _buffer.Length);
            OnBufferChanged(changeLocations);
        }

        public bool Draw(byte x, byte y, byte height, ReadOnlySpan<byte> spriteData)
        {
            byte offset = (byte)(x / 8);
            bool setToUnset = false;
            var changeLocations = new byte[256];

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
                    changeLocations[8 * (y + line) + offset] = changes;
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

                    // Write new data to buffer
                    _buffer[8 * (y + line) + offset] = (byte)(newData >> 8);
                    _buffer[8 * (y + line) + offset + 1] = (byte)(newData & 0xFF);

                    // Calculate changed pixels in this line
                    ushort changes = (ushort)(originalData ^ newData);

                    // Mark changed bytes in changes array
                    changeLocations[8 * (y + line) + offset] = (byte)(changes >> 8);
                    changeLocations[8 * (y + line) + offset + 1] = (byte)(changes & 0xFF);

                    if ((changes & originalData) != 0x0)
                        setToUnset = true;
                }
            OnBufferChanged(changeLocations);
            return setToUnset;
        }
    }
}


