using System;
using System.Threading;
using System.Threading.Tasks;
using ClientGrain;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Client
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using
                    (var client = await ConnectClient())
                {
                    await DoClientWork(client);
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while try to run client:{e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0);
            var response = await friend.SayHi("Good morning, HelloGrain!");
            Console.WriteLine("\n\n{0}\n\n", response);
            Thread.Sleep(2000);
            var f2 = client.GetGrain<IBye>(response.Guid);
            var resp2 = await f2.SayBye("ByeBye", response.Key);
            var ok = f2.Waiting();
            // IStreamProvider streamProvider = GetStreamProvider("SimpleStreamProvider");
            // IAsyncStream<T> stream = streamProvider.GetStream<T>(GetPrimaryKey(), "MyStreamNamespace");
            // StreamSubscriptionHandle<T> subscription = await stream.SubscribeAsync(IAsyncObserver<T>);

            if (ok.Result) Console.WriteLine("\n\n{0}\n\n", resp2);

            var startStream = await f2.StartStream();
            Thread.Sleep(2000);

            var streamProvider = client.GetStreamProvider("testStream");
            var asyncStream = streamProvider.GetStream<string>(f2.GetPrimaryKey(), startStream);
            var receiver = new Receiver();
            await receiver.StartReceiving(asyncStream);

            const string streamNamespace = "cSendStream";
            var streamId = new Guid();
            var stream = streamProvider.GetStream<string>(streamId, streamNamespace);
            await f2.ReceiveClientSend(streamId, streamNamespace);
            for (var i = 0; i < 10000; i++)
            {
                await stream.OnNextAsync("client:" + i);
                Thread.Sleep(1000);
            }
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            var client = new ClientBuilder().UseLocalhostClustering().Configure<ClusterOptions>(
                    opt =>
                    {
                        opt.ClusterId = "dev";
                        opt.ServiceId = "orleansTry";
                    }).ConfigureLogging(log =>
                    log.AddConsole())
                .AddSimpleMessageStreamProvider("testStream", opt => { opt.FireAndForgetDelivery = true; })
                .Build();
            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
    }
}