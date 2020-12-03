
using Google.Protobuf;

public class NetMsgData<T>  where T: IMessage<T>
{
    
    public ushort id { get; set; }
    public ushort len { get; set; }
    public T msg;

    public NetMsgData(ushort id, T msg, ushort len)
    {
        id = id;
        msg = msg;
        len = len;
    }
}

