using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

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
            Thread.Sleep(Config.OperationDuration);
            return Config;
        }

        [Route("sync/withtimeout")]
        [TimeoutMilliseconds(50)]
        public SlowResponseConfig GetConfigSlowSyncWithShortTimeout()
        {
            Thread.Sleep(Config.OperationDuration);
            return Config;
        }

        private SlowResponseConfig Config => new SlowResponseConfig
        {
            OperationDuration = _operationDuration
        };
    }
}
