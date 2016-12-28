using System;
using System.Collections.Concurrent;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.BreakCache
{
    public class CircuitBreakCache
    {
        private static readonly object Lock = new object();
        private static volatile CircuitBreakCache _singleton;
        private readonly ConcurrentDictionary<string, CircuitBreakCacheEntry> _cache;

        private CircuitBreakCache()
        {
            _cache = new ConcurrentDictionary<string, CircuitBreakCacheEntry>();
        }

        public bool ShouldTimeoutImmediately(ICircuitBreakPolicy policy, DateTime now)
        {
            if (!policy.CircuitBreakCount.HasValue)
            {
                return false;
            }
            return _cache.GetOrAdd(policy.Id, id => new CircuitBreakCacheEntry()).ShouldTimeoutImmediately(policy, now);
        }

        public void RecordTimeout(ICircuitBreakPolicy policy, DateTime now)
        {
            if (!policy.CircuitBreakCount.HasValue)
            {
                return;
            }
            _cache.GetOrAdd(policy.Id, id => new CircuitBreakCacheEntry()).IncreaseCount(now);
        }

        public static CircuitBreakCache Singleton
        {
            get
            {
                if (_singleton != null)
                {
                    return _singleton;
                }
                lock (Lock)
                {
                    if (_singleton != null)
                    {
                        return _singleton;
                    }
                    _singleton = new CircuitBreakCache();
                }
                return _singleton;
            }
        }
    }
}
