﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Skewwhiffy.CircuitBreak.BreakCache;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public static class TimeoutTaskCollectionInstantiator
    {
        public static TimeoutTaskCollection<T> TaskCollection<T>(
               this ICircuitBreakPolicy policy,
               Task<T> func,
               Action onTimeout)
        {
            return new TimeoutTaskCollection<T>(policy, func, onTimeout);
        }
        public static TimeoutTaskCollection TaskCollection(
               this ICircuitBreakPolicy policy,
               Task func,
               Action onTimeout)
        {
            return new TimeoutTaskCollection(policy, func, onTimeout);
        }

        public static TimeoutTaskCollection<T> TaskCollection<T>(
               this ICircuitBreakPolicy policy,
               Func<CancellationToken, Task<T>> func,
               Action onTimeout)
        {
            var tokenSource = new CancellationTokenSource(policy.GetTimeout());
            return policy.TaskCollection(Task.Run(async () => await func(tokenSource.Token), tokenSource.Token), onTimeout);
        }

        public static TimeoutTaskCollection TaskCollection(
               this ICircuitBreakPolicy policy,
               Func<CancellationToken, Task> func,
               Action onTimeout)
        {
            var tokenSource = new CancellationTokenSource(policy.GetTimeout());
            return policy.TaskCollection(Task.Run(async () => await func(tokenSource.Token), tokenSource.Token), onTimeout);
        }

        public static TimeoutTaskCollection TaskCollection(
            this ICircuitBreakPolicy policy,
            Action func,
            Action onTimeout)
        {
            var tokenSource = new CancellationTokenSource(policy.GetTimeout());
            return policy.TaskCollection(Task.Run(func, tokenSource.Token), onTimeout);
        }

        public static TimeoutTaskCollection<T> TaskCollection<T>(
            this ICircuitBreakPolicy policy,
            Func<T> func,
            Action onTimeout)
        {
            var tokenSource = new CancellationTokenSource(policy.GetTimeout());
            return policy.TaskCollection(Task.Run(func, tokenSource.Token), onTimeout);
        }

        public static TimeoutException GetTimeoutException(this TimeSpan timeout, Action onTimeout)
        {
            onTimeout?.Invoke();
            return new TimeoutException($"Method call timed out after {timeout.TotalMilliseconds}ms");
        }

        public static void CheckForCircuitBreak(this ICircuitBreakPolicy policy, Action onTimeout)
        {
            var circuitBreakCache = CircuitBreakCache.Singleton;
            if (circuitBreakCache.ShouldTimeoutImmediately(policy, DateTime.UtcNow))
            {
                throw policy.GetTimeout().GetTimeoutException(onTimeout);
            }
        }

        public static TimeSpan GetTimeout(this ICircuitBreakPolicy policy)
        {
            if (!policy.Timeout.HasValue)
            {
                throw new ArgumentNullException();
            }
            return policy.Timeout.Value;
        }
    }
}
