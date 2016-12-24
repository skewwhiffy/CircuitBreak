using System.Web.Http;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite.Controllers
{
    public class QuickResponseController : ApiController
    {
        public string Get()
        {
            return "Hello world";
        }
    }
}
