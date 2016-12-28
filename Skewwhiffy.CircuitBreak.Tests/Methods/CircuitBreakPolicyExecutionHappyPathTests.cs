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
    public class CircuitBreakPolicyExecutionHappyPathTests
    {
        private TimeSpan _timeout;
        private TimeSpan _operationDuration;
        private ICircuitBreakPolicy _policy;

        [SetUp]
        public void BeforeEach()
        {
            _timeout = TimeSpan.FromMilliseconds(1000);
            _operationDuration = TimeSpan.FromMilliseconds(10);
            _policy = ACircuitBreakPolicy.WithTimeout(_timeout).WithoutCircuitBreak();
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
        public void WhenSyncMethodWithNoReturnValueTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            var executed = false;
            var sw = Stopwatch.StartNew();
            _policy.ApplyTo(() =>
            {
                Thread.Sleep(_operationDuration);
                executed = true;
            });
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
            Assert.That(executed, Is.True);
        }

        [Test]
        public async Task WhenAsyncMethodTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            var sw = Stopwatch.StartNew();
            await _policy.ApplyToAsync(async () =>
            {
                await Task.Delay(_operationDuration);
                return true;
            });
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
        }

        [Test]
        public async Task WhenAsyncMethodWithNoReturnValueTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            var executed = false;
            var sw = Stopwatch.StartNew();
            await _policy.ApplyToAsync(async () =>
            {
                await Task.Delay(_operationDuration);
                executed = true;
            });
            Assert.That(executed, Is.True);
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
        }

        [Test]
        public async Task WhenCancellationTokenExists_WhenAsyncMethodTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            bool? methodCompleted = null;
            await _policy.ApplyToAsync(async t =>
            {
                methodCompleted = false;
                await Task.Delay(_operationDuration, t);
                return methodCompleted = true;
            });
            Assert.That(methodCompleted, Is.True);
        }

        [Test]
        public async Task WhenCancellationTokenExists_WhenAsyncMethodWithoutReturnValueTakesLessTimeThanTimeout_ThenMethodExecutes()
        {
            bool methodCompleted = false;
            await _policy.ApplyToAsync(async t =>
            {
                methodCompleted = false;
                await Task.Delay(_operationDuration, t);
                methodCompleted = true;
            });
            Assert.That(methodCompleted, Is.True);
        }
    }
}
