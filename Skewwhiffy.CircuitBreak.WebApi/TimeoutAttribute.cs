using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Skewwhiffy.CircuitBreak.WebApi
{
    public abstract class TimeoutAttribute : ActionFilterAttribute
    {
        private readonly TimeSpan _timeout;
        private HttpActionContext _actionContext;
        private HttpResponseMessage _response;

        protected TimeoutAttribute(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _actionContext = actionContext;
            Task.Run(KillAfterTimeout);
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            context.Response = _response ?? _actionContext.Response;
        }

        private async Task KillAfterTimeout()
        {
            await Task.Delay(_timeout);
            _response = _actionContext
                .Request
                .CreateErrorResponse(
                    HttpStatusCode.RequestTimeout,
                    "Took too long");
        }
    }

    public class TimeoutMillisecondsAttribute : TimeoutAttribute
    {
        public TimeoutMillisecondsAttribute(int milliseconds) : base(TimeSpan.FromMilliseconds(milliseconds))
        {
        }
    }
}
