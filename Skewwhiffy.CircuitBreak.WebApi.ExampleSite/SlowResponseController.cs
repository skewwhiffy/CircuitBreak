using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Skewwhiffy.CircuitBreak.Methods;
using Skewwhiffy.CircuitBreak.Policy;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite
{
    [RoutePrefix("slowresponse")]
    public class SlowResponseController : ApiController
    {
        private readonly TimeSpan _operationDuration = TimeSpan.FromMilliseconds(500);

        [Route("config")]
        public SlowResponseConfig GetConfig() => Config;

        [Route("sync")]
        public SlowResponseConfig GetConfigSlowSync()
        {
            Thread.Sleep(Config.OperationDuration);
            return Config;
        }

        [Route("async")]
        public async Task<SlowResponseConfig> GetConfigSlowAsync()
        {
            await Task.Delay(Config.OperationDuration);
            return Config;
        }

        [Route("sync/withtimeout")]
        public SlowResponseConfig GetConfigSlowSyncWithShortTimeout()
        {
            return Config
                .Policy
                .ApplyToWeb(GetConfigSlowSync);
        }

        [Route("async/withtimeout")]
        public async Task<SlowResponseConfig> GetConfigSlowAsyncWithShortTimeout()
        {
            return await Config
                .Policy
                .ApplyToWebAsync(GetConfigSlowAsync);
        }

        private SlowResponseConfig Config => new SlowResponseConfig
        {
            OperationDuration = _operationDuration,
            Policy = ACircuitBreakPolicyForWebApi
                .WithTimeout(TimeSpan.FromMilliseconds(50))
                .CircuitBreakAfterAttempts(5)
        };
    }
}
