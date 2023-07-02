namespace Chip8EmulationCore.IO
{
    /// <summary>
    /// Interface for the Chip-8 controller. This controller is a 4x4 keypad
    /// 
    /// 1 2 3 C
    /// 4 5 6 D
    /// 7 8 9 E
    /// A 0 B F
    /// </summary>
    public interface IKeyPad
    {
        /// <summary>
        /// Check if the specified key from the Chip-8 controller is pressed.
        /// </summary>
        /// <param name="key">must be a valid byte between 0x0 and 0xF</param>
        /// <returns></returns>
        bool IsKeyPressed(byte key);

        /// <summary>
        /// Wait until the next key is pressed, then return the code of the pressed key
        /// </summary>
        /// <returns></returns>
        Task<byte> AwaitNextKey();
    }
}
