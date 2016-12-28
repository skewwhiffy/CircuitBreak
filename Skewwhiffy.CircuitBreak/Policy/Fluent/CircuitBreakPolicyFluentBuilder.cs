using System;
using System.Runtime.CompilerServices;
using Skewwhiffy.CircuitBreak.Extensions;

namespace Skewwhiffy.CircuitBreak.Policy.Fluent
{
    public static class CircuitBreakHere
    {
        public static ICircuitBreakPolicy WithAttributes(
            TimeSpan timeout,
            int count,
            TimeSpan circuitBreakTimeout,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            return ACircuitBreakPolicy
                .ForCurrentMethod(filePath, lineNumber)
                .WithTimeout(timeout)
                .CircuitBreakAfterAttempts(count)
                .ReconnectAfter(circuitBreakTimeout);
        }
    }

    public static class ACircuitBreakPolicy
    {
        public static INeedTimeout ForCurrentMethod(
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            return $"{filePath}.[{lineNumber}]"
                .Pipe(WithId);
        }

        public static INeedCircuitBreakCount WithTimeout(TimeSpan timeout)
        {
            return new Builder().WithTimeout(timeout);
        }

        private static Builder WithId(string id)
        {
            return new Builder().WithId(id);
        }
    }
}
