using System;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite
{
    public class SlowResponseConfig
    {
        public TimeSpan OperationDuration { get; set; }
    }
}
