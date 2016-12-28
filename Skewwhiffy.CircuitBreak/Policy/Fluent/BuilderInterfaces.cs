using System;

namespace Skewwhiffy.CircuitBreak.Policy.Fluent
{
    public interface INeedTimeout
    {
        INeedCircuitBreakCount WithTimeout(TimeSpan timeout);
    }

    public interface INeedCircuitBreakCount
    {
        INeedReconnectionTimeout CircuitBreakAfterAttempts(int count);
        ICircuitBreakPolicy WithoutCircuitBreak();
    }

    public interface INeedReconnectionTimeout
    {
        ICircuitBreakPolicy ReconnectAfter(TimeSpan timeout);
    }
}
