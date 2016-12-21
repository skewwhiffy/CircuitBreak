using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak
{
    public static class CircuitBreakPolicyExecutor
    {
        public static async Task<T> ApplyTo<T>(this CircuitBreakPolicy policy, Func<Task<T>> func)
        {
            if (!policy.Timeout.HasValue)
            {
                return await func();
            }
            var sw = Stopwatch.StartNew();
            var task = Task.Run(async () => await func());
            while (true)
            {
                if (sw.Elapsed > policy.Timeout.Value)
                {
                    throw policy.GetTimeoutException();
                }
                if (task.IsFaulted)
                {
                    throw task.Exception?.SingleOrRaw() ?? new InvalidOperationException("Task faulted, no exception");
                }
                if (task.IsCompleted)
                {
                    return task.Result;
                }
            }
        }

        public static async Task<T> ApplyTo<T>(this CircuitBreakPolicy policy, Func<CancellationToken, Task<T>> func)
        {
            return await policy.ApplyTo(async () =>
            {
                var tokenSource = new CancellationTokenSource();
                if (policy.Timeout.HasValue)
                {
                    tokenSource.CancelAfter(policy.Timeout.Value);
                }
                var sw = Stopwatch.StartNew();
                try
                {
                    var ret = await func(tokenSource.Token);
                    if (policy.Timeout.HasValue && sw.Elapsed > policy.Timeout.Value)
                    {
                        throw policy.GetTimeoutException();
                    }
                    return ret;
                }
                catch (TaskCanceledException)
                {
                    if (!policy.Timeout.HasValue)
                    {
                        throw;
                    }
                    throw policy.GetTimeoutException();
                }
            });
        }

        private static TimeoutException GetTimeoutException(this CircuitBreakPolicy policy)
        {
            if (!policy.Timeout.HasValue)
            {
                throw new ArgumentNullException();
            }
            return new TimeoutException($"Method call timed out after {policy.Timeout.Value.TotalMilliseconds}ms");
        }

        private static Exception SingleOrRaw(this AggregateException ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}
