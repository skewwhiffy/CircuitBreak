using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public static class CircuitBreakPolicyExecutor
    {
        #region Method with no return value

        public static void ApplyTo(this ICircuitBreakPolicy policy, Action func, Action onTimeout = null)
        {
            if (!policy.Timeout.HasValue)
            {
                func();
                return;
            }
            policy.TaskCollection(func, onTimeout).GetResult();
        }

        public static async Task ApplyToAsync(this ICircuitBreakPolicy policy, Func<Task> func)
        {
            await policy.ApplyToAsync(t => func());
        }

        public static async Task ApplyToAsync(this ICircuitBreakPolicy policy, Func<CancellationToken, Task> func)
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

        public static T ApplyTo<T>(this ICircuitBreakPolicy policy, Func<T> func, Action onTimeout = null)
        {
            return !policy.Timeout.HasValue ? func() : policy.TaskCollection(func, onTimeout).GetResult();
        }

        public static T ApplyToWeb<T>(this ICircuitBreakPolicy policy, Func<T> func)
        {
            return policy.ApplyTo(func, ThrowWebTimeout);
        }

        public static async Task<T> ApplyToAsync<T>(this ICircuitBreakPolicy policy, Func<Task<T>> func, Action onTimeout = null)
        {
            return await policy.ApplyToAsync(t => func(), onTimeout);
        }

        public static async Task<T> ApplyToWebAsync<T>(this ICircuitBreakPolicy policy, Func<Task<T>> func)
        {
            return await policy.ApplyToAsync(t => func(), ThrowWebTimeout);
        }

        public static async Task<T> ApplyToAsync<T>(this ICircuitBreakPolicy policy, Func<CancellationToken, Task<T>> func, Action onTimeout = null)
        {
            if (!policy.Timeout.HasValue)
            {
                return await func(default(CancellationToken));
            }
            return policy.TaskCollection(func, onTimeout).GetResult();
        }

        #endregion

        private static void ThrowWebTimeout()
        {
            throw new HttpResponseException(HttpStatusCode.RequestTimeout);
        }
    }
}
