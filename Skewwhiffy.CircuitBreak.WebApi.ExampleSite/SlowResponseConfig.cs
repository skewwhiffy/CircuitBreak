using System;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite
{
    public class SlowResponseConfig
    {
        public TimeSpan OperationDuration { get; set; }
        public ICircuitBreakPolicy Policy { get; set; }
    }
}
