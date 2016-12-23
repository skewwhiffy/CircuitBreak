using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Skewwhiffy.CircuitBreak.Methods;

namespace Skewwhiffy.CircuitBreak.Tests.Methods
{
    public class CircuitBreakPolicyExecutionMultipleTimeoutTests
    {
        private CircuitBreakPolicy _policy;
        private const int BreakAfter = 5;
        private readonly TimeSpan _reconnectAfter = TimeSpan.FromMilliseconds(50);
        private readonly TimeSpan _operationLength = TimeSpan.FromMilliseconds(1000);
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(10);

        [SetUp]
        public void BeforeEach()
        {
            _policy = new CircuitBreakPolicy
            {
                Timeout = _timeout,
                BreakAfter = BreakAfter,
                ReconnectAfter = _reconnectAfter
            };
        }

        [Test]
        public async Task WhenTimeoutsHappenMultipleTimesInAsyncMethod_ThenCircuitBreaks_AndCircuitReconnects()
        {
            for (var i = 0; i < BreakAfter + 10; i++)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await _policy.ApplyToAsync(async () => await Task.Delay(_operationLength));
                    Assert.Fail("Expected timeout exception");
                }
                catch (TimeoutException)
                {
                }
                if (i > BreakAfter)
                {
                    Assert.That(sw.Elapsed, Is.LessThan(_policy.Timeout), $"{i}");
                }
            }

            await Task.Delay(_reconnectAfter);

            var finalSw = Stopwatch.StartNew();
            try
            {
                await _policy.ApplyToAsync(async () => await Task.Delay(_operationLength));
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
            Assert.That(finalSw.Elapsed, Is.GreaterThan(_policy.Timeout));
        }

        [Test]
        public async Task WhenTimeoutsHappenMultipleTimesInSyncMethod_ThenCircuitBreaks_AndCircuitReconnects()
        {
            for (var i = 0; i < BreakAfter + 10; i++)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    _policy.ApplyTo(() => Thread.Sleep(_operationLength));
                    Assert.Fail($"Expected timeout exception on run {i}");
                }
                catch (TimeoutException)
                {
                }
                if (i > BreakAfter)
                {
                    Assert.That(sw.Elapsed, Is.LessThan(_policy.Timeout), $"Run {i}");
                }
            }

            await Task.Delay(_reconnectAfter);

            var finalSw = Stopwatch.StartNew();
            try
            {
                _policy.Flag = "HELLO";
                _policy.ApplyTo(() => Thread.Sleep(_operationLength));
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
            Assert.That(finalSw.Elapsed, Is.GreaterThan(_policy.Timeout));
        }
    }
}
