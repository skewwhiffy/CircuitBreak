using System;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public static class CircuitBreakPolicyExecutor
    {
        public static T ApplyTo<T>(this CircuitBreakPolicy policy, Func<T> func)
        {
            return !policy.Timeout.HasValue ? func() : policy.TaskCollection(func).GetResult();
        }

        public static async Task<T> ApplyTo<T>(this CircuitBreakPolicy policy, Func<Task<T>> func)
        {
            return await policy.ApplyTo(t => func());
        }

        public static async Task<T> ApplyTo<T>(this CircuitBreakPolicy policy, Func<CancellationToken, Task<T>> func)
        {
            if (!policy.Timeout.HasValue)
            {
                return await func(default(CancellationToken));
            }
            return policy.TaskCollection(func).GetResult();
        }
    }
}
