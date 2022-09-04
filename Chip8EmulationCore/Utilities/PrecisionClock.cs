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
        public void BlockUntilElapsed(long ms, bool stopClock = true)
        {
            if (!_blockUntilElapsedSw.IsRunning) throw new ElapsedTimerNotStartedException();
            DelayLogic(_blockUntilElapsedSw, ms, stopClock);
        }

        private static void DelayLogic(Stopwatch sw, long ms, bool stopClock)
        {
            while (sw.ElapsedMilliseconds < ms)
                if (ms - sw.ElapsedMilliseconds > 75)
                    Thread.Sleep((int)Math.Max(0, Math.Min(int.MaxValue, ms - sw.ElapsedMilliseconds)) - 50);
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
