using System;
using System.Threading.Tasks;
using Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Silo
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }


        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("\n\n\n Press Enter to end....\n\n");
                Console.ReadLine();

                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder().UseLocalhostClustering().Configure<ClusterOptions>(opt =>
                {
                    opt.ClusterId = "dev";
                    opt.ServiceId = "orleansTry";
                }).ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(HelloGrain).Assembly)
                        .AddApplicationPart(typeof(ByeGrain).Assembly)
                        .WithReferences()).ConfigureLogging(log => { log.AddConsole(); })
                .AddSimpleMessageStreamProvider("testStream", opt => { opt.FireAndForgetDelivery = true; })
                .AddMemoryGrainStorage("PubSubStore", opt => { opt.NumStorageGrains = 100; });

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}