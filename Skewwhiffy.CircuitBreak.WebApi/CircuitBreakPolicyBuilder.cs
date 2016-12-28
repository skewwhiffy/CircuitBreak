using System;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.WebApi
{
    public static class ACircuitBreakPolicyForWebApi
    {
        public static ICircuitBreakPolicyBuilder WithTimeout(TimeSpan timeout)
        {
            return new CircuitBreakPolicy
            {
                Timeout = timeout
            };
        }
    }
}
