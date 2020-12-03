
using ProtoBuf;

[ProtoContract]
public class NetMsgData
{
    [ProtoMember(1)]
    public ushort ID { get; set; }
    [ProtoMember(2)]
    public string Data { get; set; }

    public NetMsgData(ushort id, string data)
    {
        ID = id;
        Data = data;
    }
}

