using System;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.Tests.WebApi
{
    public class CircuitBreakPolicyImpl : ICircuitBreakPolicy
    {
        public string Id { get; set; }
        public TimeSpan? Timeout { get; set; }
        public int? BreakAfter { get; set; }
        public TimeSpan? CircuitBreakTimeout { get; set; }
    }
}
