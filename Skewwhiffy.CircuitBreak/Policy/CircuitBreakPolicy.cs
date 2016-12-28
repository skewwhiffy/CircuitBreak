using System;

namespace Skewwhiffy.CircuitBreak.Policy
{
    public class CircuitBreakPolicy : ICircuitBreakPolicy
    {
        private string _id;

        public string Id
        {
            get { return _id ?? (_id = Guid.NewGuid().ToString()); }
            set
            {
                if (_id != null)
                {
                    throw new InvalidOperationException($"Id already set to {_id}");
                }
                _id = value;
            }
        }

        public TimeSpan? Timeout { get; set; }
        public int? CircuitBreakCount { get; set; }
        public TimeSpan? CircuitBreakTimeout { get; set; }
    }
}
