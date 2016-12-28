using System;

namespace Skewwhiffy.CircuitBreak.Policy
{
    public static class ACircuitBreakPolicy
    {
        public static ICircuitBreakPolicyBuilder WithTimeout(TimeSpan timeout)
        {
            return new CircuitBreakPolicy
            {
                Timeout = timeout
            };
        }

        public static ICircuitBreakPolicyBuilder CircuitBreakAfterAttempts(this ICircuitBreakPolicyBuilder builder, int breakAfter)
        {
            builder.BreakAfter = breakAfter;
            return builder;
        }

        public static ICircuitBreakPolicyBuilder ReconnectAfter(this ICircuitBreakPolicyBuilder builder, TimeSpan timeout)
        {
            builder.CircuitBreakTimeout = timeout;
            return builder;
        }
    }

    public interface ICircuitBreakPolicyBuilder : ICircuitBreakPolicy
    {
        new TimeSpan? Timeout { get; set; }
        new int? BreakAfter { get; set; }
        new TimeSpan? CircuitBreakTimeout { get; set; }
    }
}
