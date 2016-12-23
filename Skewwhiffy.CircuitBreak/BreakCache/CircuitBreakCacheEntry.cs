using System;

namespace Skewwhiffy.CircuitBreak.BreakCache
{
    public class CircuitBreakCacheEntry
    {
        private readonly object _lock = new object();
        private volatile int _count;
        private DateTime? _lastTimeout;

        public int Count => _count;

        public void IncreaseCount(DateTime now)
        {
            lock (_lock)
            {
                _count++;
                _lastTimeout = now;
            }
        }

        public void ResetCount()
        {
            lock (_lock)
            {
                _count = 0;
                _lastTimeout = null;
            }
        }

        public bool ShouldTimeoutImmediately(CircuitBreakPolicy policy, DateTime now)
        {
            if (!policy.BreakAfter.HasValue)
            {
                return false;
            }
            if (Count > policy.BreakAfter.Value)
            {
                if (_lastTimeout.HasValue
                    && policy.ReconnectAfter.HasValue
                    && _lastTimeout.Value.Add(policy.ReconnectAfter.Value) < now)
                {
                    ResetCount();
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
