using System.Diagnostics;

namespace Chip8EmulationCore.Utilities
{
    /// <summary>
    /// Accurate clock. Based on StopWatch.
    /// </summary>
    public class PrecisionClock
    {
        private readonly Stopwatch _sleepSw;
        private readonly Stopwatch _blockUntilElapsedSw;
        private static readonly int ticksPerMs = (int)Math.Round((double)Stopwatch.Frequency / 1000);

        public PrecisionClock()
        {
            _sleepSw = new Stopwatch();
            _blockUntilElapsedSw = new Stopwatch();
        }

        /// <summary>
        /// Sleep for the specifed amount of miliseconds.
        /// 
        /// Blocking operation.
        /// </summary>
        /// <param name="ms"></param>
        public void Sleep(long ms)
        {
            _sleepSw.Restart();
            DelayLogic(_sleepSw, ms, true);
        }

        public void StartElapsedTimer() => _blockUntilElapsedSw.Restart();

        /// <summary>
        /// Blocks the execution until the specified time has elapsed.
        /// </summary>
        /// <param name="ms">The number of milliseconds to block the execution.</param>
        /// <param name="stopClock">Optional. Indicates whether to stop the underlying stopwatch after the delay. Default is true.</param>
        /// <exception cref="ElapsedTimerNotStartedException">Thrown if the stopwatch has not been started before calling this method.</exception>
        public void BlockUntilElapsed(long ms, bool stopClock = true)
        {
            if (!_blockUntilElapsedSw.IsRunning) throw new ElapsedTimerNotStartedException();
            DelayLogic(_blockUntilElapsedSw, ms, stopClock);
        }

        private static void DelayLogic(Stopwatch sw, long ms, bool stopClock)
        {            

            while (sw.ElapsedTicks/ticksPerMs < ms)
                if (ms - sw.ElapsedTicks/ticksPerMs > 75) // If there is a lot of time left, sleep the thread
                    // Make sure to stop sleeping ~50 ms before delay end, as thread continuation can take a bit of time
                    Thread.Sleep((int)Math.Max(0, Math.Min(int.MaxValue, ms - sw.ElapsedTicks/ticksPerMs)) - 50);
            if (stopClock) sw.Stop();
        }

        /// <summary>
        /// Exception thrown if BlockUntilElapsed is called, without having called StartElapsedTimer to establish a starting timepoint
        /// </summary>
        public class ElapsedTimerNotStartedException : Exception
        {
            public ElapsedTimerNotStartedException()
            {
            }

            public ElapsedTimerNotStartedException(string message)
                : base(message)
            {
            }

            public ElapsedTimerNotStartedException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }


}
