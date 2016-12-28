using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skewwhiffy.CircuitBreak.BreakCache;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public class TimeoutTaskCollection
    {
        private readonly ICircuitBreakPolicy _policy;
        protected readonly TimeSpan Timeout;
        protected readonly CancellationToken Token;
        protected readonly Action OnTimeout;

        public TimeoutTaskCollection(
            ICircuitBreakPolicy policy,
            Task func,
            Action onTimeout)
        {
            OnTimeout = onTimeout;
            policy.CheckForCircuitBreak(onTimeout);
            _policy = policy;
            var timeout = policy.GetTimeout();
            var tokenSource = new CancellationTokenSource(timeout);
            Timeout = timeout;
            Func = Task.Run(() => func);
            Token = tokenSource.Token;
            TimeoutTask = Task.Run(async () => await Task.Delay(timeout, tokenSource.Token));
            Tasks = new[] {TimeoutTask, Func};
        }

        private Task TimeoutTask { get; }

        private Task Func { get; }

        public async Task WhenAny()
            => await Task.WhenAny(Tasks);

        public int WaitAny()
            => Task.WaitAny(Tasks);

        public void GetResult()
        {
            WaitAny();
            if (Func.IsFaulted)
            {
                var exception = Func.Exception;
                if (exception == null)
                {
                    throw Timeout.GetTimeoutException(OnTimeout);
                }
                if (exception.InnerException != null)
                {
                    throw exception.InnerException;
                }
                throw exception;
            }
            if (TimeoutTask.IsCompleted)
            {
                CircuitBreakCache.Singleton.RecordTimeout(_policy, DateTime.UtcNow);
                throw Timeout.GetTimeoutException(OnTimeout);
            }
        }

        private Task[] Tasks { get; }
    }

    public class TimeoutTaskCollection<T> : TimeoutTaskCollection
    {
        private readonly Task<T> _func;

        public TimeoutTaskCollection(
            ICircuitBreakPolicy policy,
            Task<T> func,
            Action onTimeout) : base(policy, func, onTimeout)
        {
            _func = func;
        }

        public new T GetResult()
        {
            base.GetResult();
            try
            {
                return _func.Result;
            }
            catch (AggregateException ex)
            {
                var exceptions = ex.InnerExceptions.Where(e => !(e is TaskCanceledException)).ToList();
                if (exceptions.Count != 1)
                {
                    throw Timeout.GetTimeoutException(OnTimeout);
                }
                throw exceptions.Single();
            }
            catch (TaskCanceledException)
            {
                throw Timeout.GetTimeoutException(OnTimeout);
            }
        }
    }
}
