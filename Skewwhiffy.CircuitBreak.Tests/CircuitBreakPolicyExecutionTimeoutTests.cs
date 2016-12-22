using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Skewwhiffy.CircuitBreak.Tests
{
    public class CircuitBreakPolicyExecutionTimeoutTests
    {
        private TimeSpan _timeout;
        private TimeSpan _operationDuration;
        private CircuitBreakPolicy _policy;

        [SetUp]
        public void BeforeEach()
        {
            _timeout = TimeSpan.FromMilliseconds(10);
            _operationDuration = TimeSpan.FromMilliseconds(500);
            _policy = new CircuitBreakPolicy
            {
                Timeout = _timeout
            };
        }

        [Test]
        public void WhenSyncMethodTakesLongerThanTimeout_ThenMethodThrowsTimeoutExceptionShortlyAfterTimeout()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _policy.ApplyTo(() =>
                {
                    Thread.Sleep(_operationDuration);
                    return true;
                });
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
            Assert.That(sw.Elapsed, Is.LessThan(_operationDuration));
        }

        [Test]
        public async Task WhenAsyncMethodTakesLongerThanTimeout_ThenMethodThrowsTimeoutExceptionShortlyAfterTimeout()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _policy.ApplyTo(async () =>
                {
                    await Task.Delay(_operationDuration);
                    return true;
                });
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
            Assert.That(sw.Elapsed, Is.LessThan(_operationDuration));
        }

        [Test]
        public async Task WhenCancellationTokenExists_ThenCancellationIsRequestedWhenTimingOut()
        {
            bool? methodCompleted = null;
            var sw = Stopwatch.StartNew();
            try
            {
                methodCompleted = await _policy.ApplyTo(async t =>
                {
                    methodCompleted = false;
                    await Task.Delay(_operationDuration, t);
                    return methodCompleted = true;
                });
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
            Assert.That(methodCompleted, Is.Not.Null);
            while (sw.Elapsed < _operationDuration)
            {
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(10);
            }
            Assert.That(methodCompleted, Is.False);
        }
    }
}
