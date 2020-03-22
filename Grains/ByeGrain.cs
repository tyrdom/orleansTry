using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;

namespace Grains
{
    public class ByeGrain : Grain, IBye
    {
        private HashSet<int> keys;
        private readonly ILogger logger;
        private int streamTickNum = 0;
        private IDisposable _registerTimer;

        public ByeGrain(ILogger<ByeGrain> aLogger)
        {
            logger = aLogger;
        }


        public Task ReceiveClientSend(Guid guid, string streamName)
        {
            var streamProvider = GetStreamProvider("testStream");
            var asyncStream = streamProvider.GetStream<string>(guid, streamName);
            asyncStream.SubscribeAsync(async (data, token) =>
                logger.LogInformation($"\n say bye bye streaming received:greeting='{data}'"));
            return Task.CompletedTask;
        }

        public Task<string> SayBye(string greeting, int key)
        {
            logger.LogInformation($"\n say bye bye msg received:greeting='{greeting}'");
            if (keys.Contains(key))
            {
                keys.Remove(key);
                return Task.FromResult($"\n client said='{greeting}',so HelloGrain say bye bye");
            }

            return Task.FromResult($"\n client said='{greeting}',but no good key");
        }

        public Task<bool> Waiting()
        {
            Thread.Sleep(500);
            return Task.FromResult(true);
        }

        public Task RecordKey(int key)
        {
            keys.Add(key);
            logger.LogInformation($"\n say bye bye msg received:key='{key}'");
            return Task.CompletedTask;
        }

        public Task<string> StartStream()
        {
            var streamProvider = GetStreamProvider("testStream");
            const string streamNamespace = "testName";
            logger.LogInformation($"\n init Stream{streamNamespace} ok");
            var asyncStream = streamProvider.GetStream<string>(this.GetPrimaryKey(), streamNamespace);
            var tuple = new Tuple<int>(10);
            _registerTimer = RegisterTimer(x =>
            {
                asyncStream.OnNextAsync("streamTest:" + streamTickNum);
                if (streamTickNum > 10000)
                {
                    streamTickNum = 0;
                }
                else
                {
                    streamTickNum++;
                }

                return Task.CompletedTask;
            }, tuple, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));

            asyncStream.OnNextAsync("streamTest");
            return Task.FromResult(streamNamespace);
        }

        public Task StopStream()
        {
            _registerTimer.Dispose();
            return Task.CompletedTask;
        }

        
        public override Task OnActivateAsync()
        {
            keys = new HashSet<int>();
            logger.LogInformation("\n init a bye bye ok");
            return base.OnActivateAsync();
        }
    }
}