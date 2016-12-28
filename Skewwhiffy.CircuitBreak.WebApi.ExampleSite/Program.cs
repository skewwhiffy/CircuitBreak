using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Owin.Hosting;

namespace Skewwhiffy.CircuitBreak.WebApi.ExampleSite
{
    class Program
    {
        static void Main()
        {
            var attempts = 5;
            while (true)
            {
                try
                {
                    string baseAddress = $"http://localhost:{FreeTcpPort()}/";
                    using (WebApp.Start<Startup>(baseAddress))
                    {
                        Console.WriteLine($"Listening on address {baseAddress}");
                        Console.WriteLine("Press any key to exit");
                        Console.ReadKey();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong opening port: {ex.Message}");
                    attempts--;
                    if (attempts < 0)
                    {
                        Console.WriteLine("Too many tries");
                        Console.WriteLine("Press any key to exit");
                        Console.Read();
                        return;
                    }
                }
            }

            Console.ReadLine();
        }
        private static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
