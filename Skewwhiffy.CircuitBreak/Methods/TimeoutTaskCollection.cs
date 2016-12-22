using System;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public class TimeoutTaskCollection
    {
        protected readonly TimeSpan Timeout;
        protected readonly CancellationToken Token;
        private Task[] _tasks;

        public TimeoutTaskCollection(
            TimeSpan timeout,
            Task func)
        {
            var tokenSource = new CancellationTokenSource(timeout);
            Timeout = timeout;
            Func = func;
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
            Console.WriteLine(WaitAny());
            if (Func.IsFaulted)
            {
                var exception = Func.Exception;
                if (exception == null)
                {
                    throw TimeoutException;
                }
                if (exception.InnerException != null)
                {
                    throw exception.InnerException;
                }
                throw exception;
            }
            if (Token.IsCancellationRequested || Func.IsCanceled)
            {
                throw TimeoutException;
            }
        }

        private Task[] Tasks { get; }

        private TimeoutException TimeoutException
            => new TimeoutException($"Method call timed out after {Timeout.TotalMilliseconds}ms");
    }
    public class TimeoutTaskCollection<T> : TimeoutTaskCollection
    {
        private readonly Task<T> _func;

        public TimeoutTaskCollection(
            TimeSpan timeout,
            Task<T> func) : base(timeout, func)
        {
            _func = func;
        }

        public new T GetResult()
        {
            base.GetResult();
            return _func.Result;
        }
    }
}
