using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skewwhiffy.CircuitBreak.Methods
{
    public class TimeoutTaskCollection
    {
        protected readonly TimeSpan Timeout;
        protected readonly CancellationToken Token;

        public TimeoutTaskCollection(
            TimeSpan timeout,
            Task func)
        {
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

        protected TimeoutException TimeoutException
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
            try
            {
                return _func.Result;
            }
            catch (AggregateException ex)
            {
                var exceptions = ex.InnerExceptions.Where(e => !(e is TaskCanceledException)).ToList();
                if (exceptions.Count != 1)
                {
                    throw TimeoutException;
                }
                throw exceptions.Single();
            }
            catch (TaskCanceledException)
            {
                throw TimeoutException;
            }
        }
    }
}
