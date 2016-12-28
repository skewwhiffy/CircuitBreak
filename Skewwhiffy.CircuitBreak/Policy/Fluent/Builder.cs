using System;

namespace Skewwhiffy.CircuitBreak.Policy.Fluent
{
    internal class Builder : INeedTimeout, INeedCircuitBreakCount, INeedReconnectionTimeout
    {
        private string _id;
        private TimeSpan? _timeout;
        private int? _circuitBreakCount;
        private TimeSpan? _circuitBreakTimeout;

        public Builder WithId(string id)
        {
            _id = id;
            return this;
        }

        public INeedCircuitBreakCount WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public INeedReconnectionTimeout CircuitBreakAfterAttempts(int count)
        {
            _circuitBreakCount = count;
            return this;
        }

        public ICircuitBreakPolicy ReconnectAfter(TimeSpan timeout)
        {
            _circuitBreakTimeout = timeout;
            return Build();
        }

        public ICircuitBreakPolicy WithoutCircuitBreak()
        {
            return Build();
        }

        private ICircuitBreakPolicy Build()
        {
            return new CircuitBreakPolicy
            {
                Id = _id,
                Timeout = _timeout,
                CircuitBreakCount = _circuitBreakCount,
                CircuitBreakTimeout = _circuitBreakTimeout
            };
        }
    }
}
