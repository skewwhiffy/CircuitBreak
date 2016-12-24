using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Skewwhiffy.CircuitBreak.Extensions;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite
{
    [RoutePrefix("quickresponse")]
    public class QuickResponseController : ApiController
    {
        [Route("sync")]
        public IEnumerable<string> Get()
        {
            return new List<string>();
        }

        [Route("async")]
        public async Task<IEnumerable<string>> GetAsync()
        {
            return await Get().Pipe(Task.FromResult);
        }
    }
}
