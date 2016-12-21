using System;

namespace Skewwhiffy.CircuitBreak
{
    public class CircuitBreakPolicy
    {
        public TimeSpan? Timeout { get; set; }
    }
}
