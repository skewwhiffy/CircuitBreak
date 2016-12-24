using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
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
        private TimeSpan _operationDuration;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var response = await "/slowresponse/config"
                    .PipeAsync(client.GetAsync)
                    .PipeAsync(async r => await r.Content.ReadAsStringAsync())
                    .Pipe(JsonConvert.DeserializeObject<SlowResponseConfig>);
                _operationDuration = response.OperationDuration;
            }
        }

        [Test]
        public async Task When_InvokingSlowResponseSync_Then_ResponseIsReceivedInGoodTime()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync("/slowresponse/sync");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(sw.Elapsed > _operationDuration);
            }
        }

        [Test]
        public async Task When_InvokingSlowResponseAsync_Then_ResponseIsReceivedInGoodTime()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync("/slowresponse/async");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(sw.Elapsed > _operationDuration);
            }
        }

        [Test]
        public async Task When_InvokingSlowResponseSyncWithTimeout_Then_ResponseIsTimeout()
        {
            using (var server = TestServer.Create<Startup>())
            using (var client = server.HttpClient)
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync("/slowresponse/sync/withtimeout");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                Console.WriteLine($"That took {sw.ElapsedMilliseconds}ms");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));
            }
        }
    }
}
