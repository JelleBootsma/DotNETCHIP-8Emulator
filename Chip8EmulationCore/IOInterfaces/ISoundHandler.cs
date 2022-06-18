namespace Chip8EmulationCore.IOInterfaces
{
    /// <summary>
    /// Simple interface for CHIP-8 sound.
    /// 
    /// All sound is either on or off. There are no tone options.
    /// Sound timer is running at 60Hz, so only 60 calls per second need to be possible for the sound handler.
    /// </summary>
    public interface ISoundHandler
    {
        /// <summary>
        /// Start playing sound.
        /// 
        /// Continues playing sound, if sound is already playing
        /// </summary>
        public void StartSound();

        /// <summary>
        /// Stop playing sound.
        /// </summary>
        public void StopSound();
    }
}
