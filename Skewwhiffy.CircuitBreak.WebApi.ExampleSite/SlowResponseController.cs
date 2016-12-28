using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Skewwhiffy.CircuitBreak.Methods;
using Skewwhiffy.CircuitBreak.Policy;
using Skewwhiffy.CircuitBreak.Policy.Fluent;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite
{
    [RoutePrefix("slowresponse")]
    public class SlowResponseController : ApiController
    {
        private readonly TimeSpan _operationDuration = TimeSpan.FromMilliseconds(1000);
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);
        private readonly TimeSpan _circuitBreakTimeout = TimeSpan.FromMilliseconds(100);
        private readonly int _circuitBreakAttempts = 5;
        private readonly SlowResponseConfig _config;

        public SlowResponseController()
        {
            _config = new SlowResponseConfig
            {
                OperationDuration = _operationDuration,
                Policy = ACircuitBreakPolicy
                    .ForCurrentMethod()
                    .WithTimeout(_timeout)
                    .CircuitBreakAfterAttempts(_circuitBreakAttempts)
                    .ReconnectAfter(_circuitBreakTimeout)
            };
        }

        [Route("config")]
        public SlowResponseConfig GetConfig() => _config;

        [Route("sync")]
        public SlowResponseConfig GetConfigSlowSync()
        {
            return ReturnConfigSlow();
        }

        [Route("async")]
        public async Task<SlowResponseConfig> GetConfigSlowAsync()
        {
            return await ReturnConfigSlowAsync();
        }

        [Route("sync/withtimeout")]
        public SlowResponseConfig GetConfigSlowSyncWithShortTimeout()
        {
            return CircuitBreakHere
                .WithAttributes(_timeout, _circuitBreakAttempts, _circuitBreakTimeout)
                .ApplyToWeb(ReturnConfigSlow);
        }

        [Route("async/withtimeout")]
        public async Task<SlowResponseConfig> GetConfigSlowAsyncWithShortTimeout()
        {
            return await CircuitBreakHere
                .WithAttributes(_timeout, _circuitBreakAttempts, _circuitBreakTimeout)
                .ApplyToWebAsync(ReturnConfigSlowAsync);
        }

        private async Task<SlowResponseConfig> ReturnConfigSlowAsync()
        {
            await Task.Delay(_operationDuration);
            return _config;
        }

        private SlowResponseConfig ReturnConfigSlow()
        {
            Thread.Sleep(_operationDuration);
            return _config;
        }
    }
}
