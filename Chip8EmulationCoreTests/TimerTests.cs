using Chip8EmulationCore.IO;
using Chip8EmulationCore.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Chip8EmulationCoreTests
{
    [TestClass()]
    public class TimerTests
    {
        [TestMethod()]
        public void SoundTimerTest()
        {
            var dummyHandler = new NoSoundHandler();
            var soundTimer = new SoundTimer(dummyHandler);
            soundTimer.Value = 60;
            Thread.Sleep(167);
            Assert.IsTrue(soundTimer.PlayingSound);
            Thread.Sleep(1000);
            Assert.IsFalse(soundTimer.PlayingSound);
        }

        [TestMethod()]
        public void DelayTimerTest()
        {
            var delayTimer = new DelayTimer();
            delayTimer.Value = 60;
            Assert.AreEqual(60, delayTimer.Value);
            Thread.Sleep(1100);
            Assert.AreEqual(0, delayTimer.Value);
        }
        [TestMethod()]
        public void DelayTimerRestartTest()
        {
            var delayTimer = new DelayTimer();
            delayTimer.Value = 60;
            Assert.AreEqual(60, delayTimer.Value);
            Thread.Sleep(2000);

            // Test if timer can be restarted
            delayTimer.Value = 60;
            Assert.AreEqual(60, delayTimer.Value);
            Thread.Sleep(1100);
            Assert.AreEqual(0, delayTimer.Value);
        }
    }
}