using Chip8EmulationCore.IOInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;

namespace Chip8EmulationCore.Tests
{
    [TestClass()]
    public class TimerTests
    {
        [TestMethod()]
        public void SoundTimerTest()
        {
            var handlerMoq = new Mock<ISoundHandler>();
            var soundTimer = new SoundTimer(handlerMoq.Object);
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
            Thread.Sleep(100);

            // Test if timer can be restarted
            delayTimer.Value = 60;
            Assert.AreEqual(60, delayTimer.Value);
            Thread.Sleep(1100);
            Assert.AreEqual(0, delayTimer.Value);
        }
    }
}