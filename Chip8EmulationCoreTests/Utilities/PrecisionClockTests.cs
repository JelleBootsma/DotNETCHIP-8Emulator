using Chip8EmulationCore.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Chip8EmulationCoreTests.Utilities
{
    [TestClass()]
    public class PrecisionClockTests
    {
        private readonly PrecisionClock instance = new PrecisionClock();


        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(77)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(5000)]
        public void SleepTest(long sleepForMs)
        {
            var sw = Stopwatch.StartNew();
            instance.Sleep(sleepForMs);
            sw.Stop();
            Assert.AreEqual(sleepForMs, sw.ElapsedMilliseconds);
        }

        [DataTestMethod]
        [DataRow(1, 3, 1000)]
        [DataRow(10, 11, 1000)]
        [DataRow(100, 500, 100000)]
        [DataRow(10, 10, 10)]
        [DataRow(10, 9, 10)]

        public void BlockUntilElapsedSyncExecutionTest(long blockUntilElapsedMs, long blockUntilElaspedMs2, long loopIterationCount)
        {
            long counter = 0;
            long counter2 = 0;
            long elapsedAfterFirst = 0;
            var sw = Stopwatch.StartNew();
            instance.StartElapsedTimer();

            while (counter < loopIterationCount)
                counter++;

            instance.BlockUntilElapsed(blockUntilElapsedMs, false);
            elapsedAfterFirst = sw.ElapsedMilliseconds;

            while (counter2 < loopIterationCount)
                counter2++;
            instance.BlockUntilElapsed(blockUntilElaspedMs2);
            sw.Stop();
            Assert.AreEqual(blockUntilElapsedMs, elapsedAfterFirst);

            // If blockUntilElaped > blockUntilElaspedMs2 then sw.ElapsedMilliseconds should be equal to blockUntilElaped
            // Because we already passed the blockuntil, so we should just continue
            if (blockUntilElapsedMs > blockUntilElaspedMs2)
                Assert.AreEqual(blockUntilElapsedMs, sw.ElapsedMilliseconds);
            else
                Assert.AreEqual(blockUntilElaspedMs2, sw.ElapsedMilliseconds);

        }

        [DataTestMethod]
        [DataRow(100, 500, 10)]
        [DataRow(50, 100, 1)]
        [DataRow(33, 67, 1)]

        public async Task BlockUntilElapsedAsyncExecutionTest(long blockUntilElapsedMs, long blockUntilElaspedMs2, int taskDelay)
        {
            long elapsedAfterFirst = 0;
            var sw = Stopwatch.StartNew();
            instance.StartElapsedTimer();

            await Task.Delay(taskDelay);

            instance.BlockUntilElapsed(blockUntilElapsedMs, false);
            elapsedAfterFirst = sw.ElapsedMilliseconds;

            await Task.Delay(taskDelay);

            instance.BlockUntilElapsed(blockUntilElaspedMs2);
            sw.Stop();
            Assert.AreEqual(blockUntilElapsedMs, elapsedAfterFirst);
            Assert.AreEqual(blockUntilElaspedMs2, sw.ElapsedMilliseconds);

        }

    }
}