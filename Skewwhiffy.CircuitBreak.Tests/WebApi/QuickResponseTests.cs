using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Skewwhiffy.CircuitBreak.WebApi.ExampleSite;

namespace Skewwhiffy.CircuitBreak.Tests.WebApi
{
    public class QuickResponseTests
    {
        [Test]
        public async Task When_InvokingQuickResponseSync_Then_ResponseIsReceived()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/quickresponse/sync");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }
        [Test]
        public async Task When_InvokingQuickResponseAsync_Then_ResponseIsReceived()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/quickresponse/async");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }
    }
}
