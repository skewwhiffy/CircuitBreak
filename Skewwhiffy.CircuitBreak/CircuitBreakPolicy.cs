using System;

namespace Skewwhiffy.CircuitBreak
{
    public class CircuitBreakPolicy
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
        public int? BreakAfter { get; set; }
        public TimeSpan? ReconnectAfter { get; set; }

        public string Flag { get; set; }
    }
}
