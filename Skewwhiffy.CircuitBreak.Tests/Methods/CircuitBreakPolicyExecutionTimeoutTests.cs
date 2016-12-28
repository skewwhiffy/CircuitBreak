using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Skewwhiffy.CircuitBreak.Methods;
using Skewwhiffy.CircuitBreak.Policy;
using Skewwhiffy.CircuitBreak.Policy.Fluent;

namespace Skewwhiffy.CircuitBreak.Tests.Methods
{
    public class CircuitBreakPolicyExecutionTimeoutTests
    {
        private TimeSpan _timeout;
        private TimeSpan _operationDuration;
        private ICircuitBreakPolicy _policy;

        [SetUp]
        public void BeforeEach()
        {
            _timeout = TimeSpan.FromMilliseconds(10);
            _operationDuration = TimeSpan.FromMilliseconds(10000);
            _policy = ACircuitBreakPolicy
                .WithTimeout(_timeout)
                .WithoutCircuitBreak();
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
        public void WhenSyncMethodWithNoReturnValueTakesLongerThanTimeout_ThenMethodThrowsTimeoutExceptionShortlyAfterTimeout()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _policy.ApplyTo(() => Thread.Sleep(_operationDuration));
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
                await _policy.ApplyToAsync(async () =>
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
            _timeout = TimeSpan.FromMilliseconds(100);
            _operationDuration = TimeSpan.FromMilliseconds(300);
            bool? methodCompleted = null;
            var sw = Stopwatch.StartNew();
            try
            {
                methodCompleted = await _policy.ApplyToAsync(async t =>
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
