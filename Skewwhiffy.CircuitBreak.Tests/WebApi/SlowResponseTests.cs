using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Skewwhiffy.CircuitBreak.Extensions;
using Skewwhiffy.CircuitBreak.WebApi.ExampleSite;

namespace Skewwhiffy.CircuitBreak.Tests.WebApi
{
    public class SlowResponseTests
    {
        private SlowResponseConfigImpl _config;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                _config = await "/slowresponse/config"
                    .PipeAsync(client.GetAsync)
                    .PipeAsync(async r => await r.Content.ReadAsStringAsync())
                    .Pipe(JsonConvert.DeserializeObject<SlowResponseConfigImpl>);
            }
        }

        [TestCase("slowresponse/sync")]
        [TestCase("slowresponse/async")]
        public async Task When_InvokingSlowResponse_Then_ResponseIsReceivedInGoodTime(string endpoint)
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync(endpoint);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(sw.Elapsed > _config.OperationDuration);
            }
        }

        [TestCase("slowresponse/sync/withtimeout")]
        [TestCase("slowresponse/async/withtimeout")]
        public async Task When_InvokingSlowResponseWithTimeout_Then_ResponseIsTimeout(string endpoint)
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync(endpoint);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));
                Assert.That(sw.Elapsed < _config.OperationDuration);
            }
        }

        [TestCase("slowresponse/sync/withtimeout")]
        [TestCase("slowresponse/async/withtimeout")]
        public async Task When_InvokingSlowResponseWithTimeoutMultipleTimes_Then_CircuitBreaks(string endpoint)
        {
            Assert.That(_config.Policy.BreakAfter.HasValue);
            Assert.That(_config.Policy.Timeout.HasValue);
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                for (var i = 0; i < _config.Policy.BreakAfter.Value + 5; i++)
                {
                    var sw = Stopwatch.StartNew();
                    var response = await client.GetAsync(endpoint);
                    if (i > _config.Policy.BreakAfter.Value)
                    {
                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));
                        Assert.That(sw.Elapsed < _config.Policy.Timeout.Value);
                    }
                }
            }
        }
    }
}
