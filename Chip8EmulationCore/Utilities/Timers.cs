using Chip8EmulationCore.IO;
using Timer = System.Timers.Timer;

namespace Chip8EmulationCore.Utilities
{
    /// <summary>
    /// Delay timer, ticks down at 60Hz, while value can be set and read.
    /// 
    /// Can be used for in-game timing events.
    /// </summary>
    public class DelayTimer
    {
        private const double TIMER_FREQUENCY = 60;
        private byte _value = 0;
        private readonly Timer _timer;

        /// <summary>
        /// Create a new DelayTimer
        /// </summary>
        public DelayTimer()
        {
            _timer = new Timer(Math.Round(1000d / TIMER_FREQUENCY));
            _timer.Elapsed += Timer_Tick;
        }

        /// <summary>
        /// Get or set the current timer value.
        /// 
        /// Timer will stop automatically if value is 0, 
        /// and starts again automatically when value is no longer 0.
        /// </summary>
        public byte Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _timer.Start();
            }
        }
        protected virtual void Timer_Tick(object? sender, EventArgs args)
        {
            if (Value == 0)
            {
                _timer.Stop();
                return;
            }
            _value--;
        }

    }

    /// <summary>
    /// Sound timer, ticks down at 60Hz.
    /// 
    /// While value > 0, the soundhandler will make sound, 
    /// and automatically stop when value reaches 0
    /// </summary>
    public class SoundTimer : DelayTimer
    {
        private readonly ISoundHandler _soundHandler;
        private bool _playingSound = false;

        /// <summary>
        /// True if value > 0
        /// </summary>
        public bool PlayingSound { get { return _playingSound; } }
        public SoundTimer(ISoundHandler soundHandler) : base()
        {
            _soundHandler = soundHandler ?? throw new ArgumentNullException(nameof(soundHandler));
        }

        protected override void Timer_Tick(object? sender, EventArgs e)
        {
            base.Timer_Tick(sender, e);
            if (Value > 0 && !_playingSound)
            {
                _playingSound = true;
                _soundHandler.StartSound();
                return;
            }
            if (Value == 0 && _playingSound)
            {
                _playingSound = false;
                _soundHandler.StopSound();
                return;
            }
        }
    }

}
