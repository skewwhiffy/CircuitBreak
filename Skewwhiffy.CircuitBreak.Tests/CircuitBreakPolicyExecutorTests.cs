using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Skewwhiffy.CircuitBreak.Tests
{
    public class CircuitBreakPolicyExecutorTests
    {
        [Test]
        public async Task PolicyTimeoutIsRespectedForSyncMethodWithReturnValue()
        {
            var policy = new CircuitBreakPolicy
            {
                Timeout = TimeSpan.FromMilliseconds(100)
            };
            try
            {
                await policy.ApplyTo(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    return true;
                });
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
        }

        [Test]
        public async Task TokenProvidedCancelsTaskIfMethodCallAllowsTokenToBePassedThrough()
        {
            bool methodCompleted = false;
            var policy = new CircuitBreakPolicy
            {
                Timeout = TimeSpan.FromMilliseconds(100)
            };
            var sw = Stopwatch.StartNew();
            try
            {
                methodCompleted = await policy.ApplyTo(async t =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200), t);
                    return methodCompleted = true;
                });
                Assert.Fail("Expected timeout exception");
            }
            catch (TimeoutException)
            {
            }
            while (sw.Elapsed < TimeSpan.FromMilliseconds(300))
            {
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(10);
            }
            Assert.That(methodCompleted, Is.False);
        }
    }
}
