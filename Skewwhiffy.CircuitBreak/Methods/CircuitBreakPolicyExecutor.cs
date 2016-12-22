using System;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public static class CircuitBreakPolicyExecutor
    {
        #region Method with no return value

        public static void ApplyTo(this CircuitBreakPolicy policy, Action func)
        {
            if (!policy.Timeout.HasValue)
            {
                func();
                return;
            }
            policy.TaskCollection(func).GetResult();
        }

        public static async Task ApplyToAsync(this CircuitBreakPolicy policy, Func<Task> func)
        {
            await policy.ApplyToAsync(t => func());
        }

        public static async Task ApplyToAsync(this CircuitBreakPolicy policy, Func<CancellationToken, Task> func)
        {
            Func<CancellationToken, Task<bool>> funcWithDummyReturnValue = async t =>
            {
                await func(t);
                return true;
            };
            await policy.ApplyToAsync(funcWithDummyReturnValue);
        }

        #endregion

        #region Method with return value

        public static T ApplyTo<T>(this CircuitBreakPolicy policy, Func<T> func)
        {
            return !policy.Timeout.HasValue ? func() : policy.TaskCollection(func).GetResult();
        }

        public static async Task<T> ApplyToAsync<T>(this CircuitBreakPolicy policy, Func<Task<T>> func)
        {
            return await policy.ApplyToAsync(t => func());
        }

        public static async Task<T> ApplyToAsync<T>(this CircuitBreakPolicy policy, Func<CancellationToken, Task<T>> func)
        {
            if (!policy.Timeout.HasValue)
            {
                return await func(default(CancellationToken));
            }
            return policy.TaskCollection(func).GetResult();
        }

        #endregion
    }
}
