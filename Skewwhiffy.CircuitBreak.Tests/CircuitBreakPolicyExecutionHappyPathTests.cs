using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Skewwhiffy.CircuitBreak.Tests
{
    public class CircuitBreakPolicyExecutionHappyPathTests
    {
        private TimeSpan _timeout;
        private TimeSpan _operationDuration;
        private CircuitBreakPolicy _policy;

        [SetUp]
        public void BeforeEach()
        {
            _timeout = TimeSpan.FromMilliseconds(100);
            _operationDuration = TimeSpan.FromMilliseconds(10);
            _policy = new CircuitBreakPolicy
            {
                Timeout = _timeout
            };
        }

        [Test]
        public void WhenSyncMethodTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            var sw = Stopwatch.StartNew();
            _policy.ApplyTo(() =>
            {
                Thread.Sleep(_operationDuration);
                return true;
            });
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
        }

        [Test]
        public async Task WhenAsyncMethodTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            var sw = Stopwatch.StartNew();
            await _policy.ApplyTo(async () =>
            {
                await Task.Delay(_operationDuration);
                return true;
            });
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
        }

        [Test]
        public async Task WhenCancellationTokenExists_WhenAsyncMethodTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            bool? methodCompleted = null;
            methodCompleted = await _policy.ApplyTo(async t =>
            {
                methodCompleted = false;
                await Task.Delay(_operationDuration, t);
                return methodCompleted = true;
            });
            Assert.That(methodCompleted, Is.True);
        }
    }
}
