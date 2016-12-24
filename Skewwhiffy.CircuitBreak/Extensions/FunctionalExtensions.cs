using System;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak.Extensions
{
    public static class FunctionalExtensions
    {
        public static TTo Pipe<TFrom, TTo>(this TFrom from, Func<TFrom, TTo> getTo)
        {
            return getTo(from);
        }
        public static async Task<TTo> Pipe<TFrom, TTo>(this Task<TFrom> from, Func<TFrom, TTo> getTo)
        {
            return getTo(await from);
        }

        public static async Task<TTo> PipeAsync<TFrom, TTo>(this TFrom from, Func<TFrom, Task<TTo>> getTo)
        {
            return await getTo(from);
        }

        public static async Task<TTo> PipeAsync<TFrom, TTo>(this Task<TFrom> from, Func<TFrom, Task<TTo>> getTo)
        {
            return await getTo(await from);
        }
    }
}
