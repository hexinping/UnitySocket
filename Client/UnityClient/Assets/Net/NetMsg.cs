using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public delegate void NetHandler(IMessage msgData, ushort msgId);


/// <summary>
/// 业务和socket的中间层
/// 业务通过这个类和socket交互
/// </summary>
public static class NetMsg
{
    private static Dictionary<ushort, NetHandler> m_EventMap = new Dictionary<ushort, NetHandler>();
    private static Dictionary<int, Socket> m_socketMap = new Dictionary<int, Socket>();


    // 发送请求
    public static void SendMsg(Socket socket, IMessage data, ushort id)
    {
        var content = data.ToByteArray();
        var byteArray = ProtoBufUtil.Encode(content, id);
        socket.BeginSend(byteArray, 0, byteArray.Length, SocketFlags.None, null, null);
        Debug.Log($"[Client]client send: ID:{id},DataLen:{content.Length}");
    }

    //// 派发
    public static void HandleMsg<T>(byte[] buffer) where T : IMessage<T>, new()
    {
        ushort msgId = 0;
        T data = ProtoBufUtil.Uncode<T>(buffer, out msgId);
        var protoID = msgId;
        if (m_EventMap.ContainsKey(protoID))
        {
            var callback = m_EventMap[protoID];
            Debug.Log($"[Client] receive ：protoID：{protoID}，dataLen：{buffer.Length - 4}");
            callback?.Invoke(data, msgId);
        }

    }


    // 移除监听
    public static void RemoveListener(ushort protoID)
    {
        if (m_EventMap.ContainsKey(protoID))
        {
            m_EventMap[protoID] = null;
            m_EventMap.Remove(protoID);
        }
    }

    // 监听 暂时只允许一个地方监听一条服务器消息
    public static void AddListener(ushort protoID, NetHandler callBack)
    {
        if (!m_EventMap.ContainsKey(protoID))
        {
            m_EventMap.Add(protoID, null);
        }
        m_EventMap[protoID] = callBack;
    }
}