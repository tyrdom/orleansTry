using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace ClientGrain
{
    public class Receiver : IAsyncObserver<string>
    {
        private StreamSubscriptionHandle<string> _streamHandle ;

        public Task OnNextAsync(string item, StreamSequenceToken token = null)
        {
            Console.Out.WriteLine("data:" + item);
            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task StartReceiving(IAsyncStream<string> stream)
        {
            _streamHandle = await stream.SubscribeAsync(this);
        }

        public async Task StopReceiving()
        {
            await _streamHandle.UnsubscribeAsync();
        }
    }
}