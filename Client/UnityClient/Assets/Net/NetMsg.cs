using System.Collections.Generic;

public delegate void NetHandler(string msgData);

/// <summary>
/// 业务和socket的中间层
/// 业务通过这个类和socket交互
/// </summary>
public static class NetMsg
{
    private static Dictionary<int, NetHandler> m_EventMap = new Dictionary<int, NetHandler>();

    // 向服务器发送请求
    //public static void SendMsg(NetMsgData data)
    //{
    //    //ClientSocket.Instance.SendMsg(ProtoBufUtil.PackNetMsg(data));
    //    //Log.Debug($"[Client]client send: ID:{data.ID},Data:{data.Data}");
    //}

    // 派发
    public static void HandleMsg(byte[] buffer)
    {
        //NetMsgData data = ProtoBufUtil.UnpackNetMsg(buffer);
        //var protoID = data.ID;
        //if (m_EventMap.TryGetValue(protoID, out NetHandler callback))
        //{
        //    //Log.Debug($"[Server]收到 ：protoID：{protoID}，data：{data.Data}");
        //    callback?.Invoke(data.Data);
        //}
        //else
        //{
        //    //Log.Debug($"[Server]收到未监听的服务器消息：protoID：{protoID}，data：{data.Data}");
        //}
    }

    // 移除监听
    public static void RemoveListener(int protoID)
    {
        if (m_EventMap.ContainsKey(protoID))
        {
            m_EventMap[protoID] = null;
            m_EventMap.Remove(protoID);
        }
    }

    // 监听 暂时只允许一个地方监听一条服务器消息
    public static void AddListener(int protoID, NetHandler callBack)
    {
        if (!m_EventMap.ContainsKey(protoID))
        {
            m_EventMap.Add(protoID, null);
        }

        m_EventMap[protoID] = callBack;
    }
}