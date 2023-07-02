namespace Chip8EmulationCore.IO
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

    /// <summary>
    /// Dummy handler for the sound handler. Will not actually make any sound.
    /// 
    /// To be used in cases where no sound is supported / desired
    /// </summary>
    public class NoSoundHandler : ISoundHandler
    {
        public void StartSound()
        {
        }

        public void StopSound()
        {
        }
    }
}
