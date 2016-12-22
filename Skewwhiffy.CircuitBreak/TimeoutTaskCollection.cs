using System;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak
{
    public class TimeoutTaskCollection<T>
    {
        private readonly TimeSpan _timeout;
        private readonly Task<T> _func;
        private readonly CancellationToken _token;
        private Task _timeoutTask;
        private Task[] _tasks;

        public TimeoutTaskCollection(
            TimeSpan timeout,
            Task<T> func)
        {
            _timeout = timeout;
            _func = func;
            var tokenSource = new CancellationTokenSource(_timeout);
            _token = tokenSource.Token;
        }

        public async Task WhenAny()
            => await Task.WhenAny(Tasks);

        public void WaitAny()
            => Task.WaitAny(Tasks);

        public T GetResult()
        {
            WaitAny();
            if (_token.IsCancellationRequested || _func.IsFaulted || _func.IsCanceled)
            {
                throw TimeoutException;
            }
            return _func.Result;
        }

        private TimeoutException TimeoutException
            => new TimeoutException($"Method call timed out after {_timeout.TotalMilliseconds}ms");

        private Task[] Tasks
            => _tasks ?? (_tasks = new[] {TimeoutTask, _func});

        private Task TimeoutTask
            => _timeoutTask ?? (_timeoutTask = Task.Run(async () => await Task.Delay(_timeout, _token)));
    }
}
