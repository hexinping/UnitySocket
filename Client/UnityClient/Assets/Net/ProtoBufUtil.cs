
using Google.Protobuf;
using System;
using System.IO;

public class ProtoBufUtil
{
    // 封包，依次写入协议数据长度、协议id、协议内容
    public static byte[] Encode(byte[] data, ushort msgId)
    {

        ushort protoId = msgId;
        MemoryStream ms = null;
        using (ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            byte[] pbdata = data;
            ushort msglen = (ushort)pbdata.Length;
            writer.Write(msglen);
            writer.Write(protoId);
            writer.Write(pbdata);
            writer.Flush();
            return ms.ToArray();
        }
    }

    // 解包，依次写出协议数据长度、协议id、协议数据内容
    public static T Uncode<T>(byte[] msgData, out ushort msgId) where T : IMessage<T>, new()
    {
        MemoryStream ms = null;
        MessageParser<T> Parser = new MessageParser<T>(() => new T());
        T result = default(T);
        using (ms = new MemoryStream(msgData))
        {
            BinaryReader reader = new BinaryReader(ms);
            ushort msgLen = reader.ReadUInt16();
            ushort protoId = reader.ReadUInt16();
            msgId = protoId;
            byte[] pbdata = reader.ReadBytes(msgLen);
            if (msgLen <= msgData.Length - 4)
            {
                result = Parser.ParseFrom(pbdata);
            }
            else
            {
                Console.WriteLine($" {protoId} 协议长度错误");
            }
        }

        return result;
    }

}

