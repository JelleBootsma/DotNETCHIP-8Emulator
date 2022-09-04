namespace Chip8EmulationCore.IOInterfaces
{
    /// <summary>
    /// Display interface, which the Cpu can use to display things to the user.
    /// 
    /// Display has a resolution 64x32, with 0,0 being the top left pixel.
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// Clear everything on the display
        /// </summary>
        public void Clear();

        /// <summary>
        /// Draws a sprite at coordinate (x, y) that has a width of 8 pixels and a height of N pixels. 
        /// 
        /// Each row of 8 pixels is read as bit-coded from spriteData;
        /// All sprite data is XOR'ed with the pre-existing screen data;
        /// 
        /// Returns true if any screen pixels are flipped from set to unset when the sprite is drawn, and false if that does not happen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <param name="spriteData"></param>
        public bool Draw(byte x, byte y, byte height, ReadOnlySpan<byte> spriteData);
    }
}
