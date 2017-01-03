using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Skewwhiffy.CircuitBreak.Tests
{
    public class Sandbox
    {
        [Test]
        public async Task CheckStopwatchVsTaskDelay()
        {
            var tasks = Enumerable
                .Range(0, 100)
                .Select(i => CheckSingle())
                .ToArray();
            await Task.WhenAll(tasks);
        }

        private async Task CheckSingle()
        {
            var sw = Stopwatch.StartNew();
            await Task.Delay(TimeSpan.FromSeconds(6));
            sw.Stop();
            Assert.That(sw.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(6)));
        }
    }
}
