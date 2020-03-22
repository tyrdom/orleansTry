using System;
using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains
{
    public class HelloGrain : Grain, IHello
    {
        private readonly ILogger _logger;
        private int _ticket;

        public HelloGrain(ILogger<HelloGrain> aLogger)
        {
            _logger = aLogger;
        }

        public Task<GuidAndKey> SayHi(string greeting)
        {
            _logger.LogInformation($"\n sayhi msg received:greeting='{greeting}'");
            var guid = Guid.NewGuid();

            var grain = GrainFactory.GetGrain<IBye>(guid);
            _ticket++;
            grain.RecordKey(_ticket);
            return Task.FromResult(new GuidAndKey(guid, _ticket));
        }

        public override Task OnActivateAsync()
        {
            _ticket = 0;
            return base.OnActivateAsync();
        }
    }
}