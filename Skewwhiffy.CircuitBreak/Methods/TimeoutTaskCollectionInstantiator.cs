using System;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public static class TimeoutTaskCollectionInstantiator
    {
        public static TimeoutTaskCollection<T> TaskCollection<T>(
               this CircuitBreakPolicy policy,
               Task<T> func)
        {
            return new TimeoutTaskCollection<T>(policy.GetTimeout(), func);
        }

        public static TimeoutTaskCollection<T> TaskCollection<T>(
               this CircuitBreakPolicy policy,
               Func<CancellationToken, Task<T>> func)
        {
            var tokenSource = new CancellationTokenSource(policy.GetTimeout());
            return policy.TaskCollection(Task.Run(async () => await func(tokenSource.Token), tokenSource.Token));
        }

        public static TimeoutTaskCollection<T> TaskCollection<T>(
            this CircuitBreakPolicy policy,
            Func<T> func)
        {
            var tokenSource = new CancellationTokenSource(policy.GetTimeout());
            return policy.TaskCollection(Task.Run(func, tokenSource.Token));
        }

        private static TimeSpan GetTimeout(this CircuitBreakPolicy policy)
        {
            if (!policy.Timeout.HasValue)
            {
                throw new ArgumentNullException();
            }
            return policy.Timeout.Value;
        }
    }
}
