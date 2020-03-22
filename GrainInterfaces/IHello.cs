using System;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces
{
    public interface IHello : IGrainWithIntegerKey
    {
        Task<GuidAndKey> SayHi(string greeting);
    }

    [Serializable]
    public struct GuidAndKey
    {
        public Guid Guid;
        public int Key;

        public GuidAndKey(Guid guid, int key)
        {
            Guid = guid;
            Key = key;
        }
    }
}