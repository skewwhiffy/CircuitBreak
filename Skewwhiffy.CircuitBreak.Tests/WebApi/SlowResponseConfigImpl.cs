using System;

namespace Skewwhiffy.CircuitBreak.Tests.WebApi
{
    public class SlowResponseConfigImpl
    {
        public TimeSpan OperationDuration { get; set; }
        public CircuitBreakPolicyImpl Policy { get; set; }
    }
}
