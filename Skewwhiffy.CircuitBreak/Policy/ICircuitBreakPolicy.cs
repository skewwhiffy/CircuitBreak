using System;

namespace Skewwhiffy.CircuitBreak.Policy
{
    public interface ICircuitBreakPolicy
    {
        string Id { get; }
        TimeSpan? Timeout { get; }
        int? BreakAfter { get; }
        TimeSpan? CircuitBreakTimeout { get; }

    }
}
