using System;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces
{
    public interface IBye : IGrainWithGuidKey
    {
        Task<string> SayBye(string greeting, int key);

        Task<bool> Waiting();

        Task RecordKey(int key);

        Task ReceiveClientSend(Guid guid, string streamName);
        Task<string> StartStream();

        Task StopStream();
    }
}