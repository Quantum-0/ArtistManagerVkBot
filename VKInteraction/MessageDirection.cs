using ProtoBuf;

namespace VKInteraction
{
    [ProtoContract]
    public enum MessageDirection
    {
        [ProtoEnum]
        Unknown = 0,
        [ProtoEnum]
        In = 1,
        [ProtoEnum]
        Out = 2
    }
}
