using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Skewwhiffy.CircuitBreak.Methods;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.Tests.Methods
{
    public class CircuitBreakPolicyExecutionExceptionTests
    {
        private TimeSpan _timeout;
        private TimeSpan _operationDuration;
        private ICircuitBreakPolicy _policy;

        [SetUp]
        public void BeforeEach()
        {
            _timeout = TimeSpan.FromMilliseconds(1000);
            _operationDuration = TimeSpan.FromMilliseconds(10);
            _policy = ACircuitBreakPolicy.WithTimeout(_timeout);
        }

        [Test]
        public void WhenSyncMethodTakesLessTimeThanTimeout_ThenExceptionBubblesUp()
        {
            var sw = Stopwatch.StartNew();
            Assert.Throws<InvalidOperationException>(() => _policy.ApplyTo((Func<bool>)(() =>
            {
                Thread.Sleep(_operationDuration);
                throw new InvalidOperationException();
            })));
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
        }

        [Test]
        public void WhenSyncMethodWithNoReturnValueTakesLessTimeThanTimeout_ThenExceptionBubblesUp()
        {
            var sw = Stopwatch.StartNew();
            Assert.Throws<InvalidOperationException>(() => _policy.ApplyTo(() =>
            {
                Thread.Sleep(_operationDuration);
                throw new InvalidOperationException();
            }));
            Assert.That(sw.Elapsed, Is.GreaterThan(_operationDuration));
        }

        [Test]
        public async Task WhenAsyncMethodTakesLessTimeThanTimeout_ThenExceptionBubblesUp()
        {
            try
            {
                await _policy.ApplyToAsync((Func<Task<bool>>) (async () =>
                {
                    await Task.Delay(_operationDuration);
                    throw new InvalidOperationException();
                }));
                Assert.Fail("Expected exception to bubble up");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public async Task WhenAsyncMethodWithNoReturnValueTakesLessTimeThanTimeout_ThenExceptionBubblesUp()
        {
            try
            {
                await _policy.ApplyToAsync(async () =>
                {
                    await Task.Delay(_operationDuration);
                    throw new InvalidOperationException();
                });
                Assert.Fail("Expected exception to bubble up");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public async Task WhenCancellationTokenExists_WhenAsyncMethodTakesLessTimeThanTimeout_ThenExceptionBubblesUp()
        {
            bool? methodCompleted = null;
            try
            {
                methodCompleted = await _policy.ApplyToAsync<bool>(async t =>
                {
                    methodCompleted = false;
                    await Task.Delay(_operationDuration, t);
                    throw new InvalidOperationException();
                });
                Assert.Fail("Expected exception to bubble up");
            }
            catch (InvalidOperationException)
            {
            }
            Assert.That(methodCompleted, Is.False);
        }

        [Test]
        public async Task WhenCancellationTokenExists_WhenAsyncMethodWithoutReturnValueTakesLessTimeThanTimeout_ThenExceptionBubblesUp()
        {
            bool? methodCompleted = null;
            try
            {
                await _policy.ApplyToAsync(async t =>
                {
                    methodCompleted = false;
                    await Task.Delay(_operationDuration, t);
                    throw new InvalidOperationException();
                });
                Assert.Fail("Expected exception to bubble up");
            }
            catch (InvalidOperationException)
            {
            }
            Assert.That(methodCompleted, Is.False);
        }
    }
}
