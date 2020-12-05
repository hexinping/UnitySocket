using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using HxpTest.AddressBook;
using static HxpTest.AddressBook.Person.Types;
using Google.Protobuf;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading;
using PB;

public class UClient : MonoBehaviour
{

    private static byte[] buffer = new byte[1024];
    private Socket clientSocket;

    private float sendTimeIntveral = 2;
    private float lastSendTime;
    private float lastSendPingTime;
    private long lastSCPingTime;

    private object _lockObj = new object();

    public class NetData
    {
        public byte[] msg;
        public ushort msgId;
    }

    private Queue<NetData> receiveQ = new Queue<NetData>();


    // Start is called before the first frame update
    void Start()
    {

        lastSCPingTime = DateTime.Now.Ticks;
        //Debug.Log($"主线程ID {Thread.CurrentThread.ManagedThreadId}");
        AddListeners();

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket = socket;
        //连接到指定服务器的指定端口
        //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.connect.aspx
        socket.Connect("127.0.0.1", 4530);


        //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginreceive.aspx
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);

        Debug.Log("connect to the server");
    }

    public void AddListeners()
    {
        NetMsg.AddListener((ushort)PB.EnmCmdID.ScPerson, new NetHandler(Test));
        NetMsg.AddListener((ushort)PB.EnmCmdID.ScPing, new NetHandler(TestPing));

    }

    public void Test(byte[] data, ushort msgId)
    {
        Person person = Person.Parser.ParseFrom(data);
        Debug.Log($"客户端收到服务器消息 {msgId} 回调====={person.Name}");
    }

    public void TestPing(byte[] data, ushort msgId)
    {
        SCPing scping = SCPing.Parser.ParseFrom(data);
        lastSCPingTime = DateTime.Now.Ticks;
        Debug.Log($"【Client】HeartBeat 客户端收到服务器心跳 {msgId} 回调====={scping.Time}  {DateTime.Now}");
    }

    public void ReceiveMessage(IAsyncResult ar)
    {
        var clientSocket = ar.AsyncState as Socket;
        try
        {

            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
            var length = clientSocket.EndReceive(ar);
            if (length > 0)
            {
                //读取出来消息内容
                //var message = Encoding.Unicode.GetString(buffer, 0, length);
                //Debug.Log(message);
                var data = buffer.Take(length).ToArray();

                MemoryStream ms = null;
                using (ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    ushort msgLen = reader.ReadUInt16();
                    ushort protoId = reader.ReadUInt16();
                    //Debug.Log($"接收消息线程ID {Thread.CurrentThread.ManagedThreadId}");  每次都是一个子线程处理，可能不一样
                    Debug.Log($"[Client] receive ：protoID：{protoId}，dataLen：{msgLen}");

                    if (msgLen <= data.Length - 4)
                    {
                        byte[] pbdata = reader.ReadBytes(msgLen);

                        NetData netData = new NetData()
                        {
                            msg = pbdata,
                            msgId = protoId
                        };

                        lock (_lockObj)
                        {
                            receiveQ.Enqueue(netData);
                        }
                        
                        //NetMsg.HandleMsg(pbdata, protoId);
                    }
                    else
                    {
                        Debug.Log($" [Client] {protoId} 协议长度错误");
                    }
                }

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), clientSocket);
            }
            else
            {
                // 接收到长度为0的数据，代表断开连接
                OnServerDisconnect();
            }

        }
        catch (SocketException ex)
        {
            // 如果socket有未发送完的数据，断开时会触发10054异常，在此捕捉
            if (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine($"clientSocket有未发送完的数据 {ex.Message}");
                OnServerDisconnect();
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (clientSocket == null) return;


        if (Time.time - lastSendPingTime >= 1)
        {
            lastSendPingTime = Time.time;
            //心跳检测
            var elapTime = new TimeSpan(DateTime.Now.Ticks - lastSCPingTime);
            if (elapTime.Seconds > 5)
            {
                Debug.Log($"未收到服务器的心跳信息超过5秒 心跳超时===={DateTime.Now}");
            }

            CSPing csPing = new CSPing()
            {
                Time = 1
            };
            NetMsg.SendMsg(clientSocket, csPing, (ushort)PB.EnmCmdID.CsPing);
        }
            //测试code
        if (Time.time - lastSendTime >= sendTimeIntveral)
        {
            lastSendTime = Time.time;
            //var message = "Message from client : " + DateTime.Now;
            //var outputBuffer = Encoding.Unicode.GetBytes(message);

            


            //使用protobuf
            Person john = new Person
            {
                Id = 1234,
                Name = "Message from client at " + DateTime.Now.ToString(),
                Email = "jdoe@example.com",
                Phones = { new Person.Types.PhoneNumber { Number = "555-4321", Type = PhoneType.Home } }
            };

            //byte[] byteArray = john.ToByteArray();
            //byte[] byteArray = ProtoBufUtil.Encode(john.ToByteArray(), 102);
            if (clientSocket == null) return;
            //clientSocket.BeginSend(byteArray, 0, byteArray.Length, SocketFlags.None, null, null);
            NetMsg.SendMsg(clientSocket, john, (ushort)PB.EnmCmdID.CsPerson);
        }


        lock (_lockObj)
        {
            while (receiveQ.Count > 0)
            {
                NetData netData = receiveQ.Dequeue();
                NetMsg.HandleMsg(netData.msg, netData.msgId);
            }
        }
    }
    private void OnServerDisconnect()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
            clientSocket = null;
        }
       
    }

    private void OnDestroy()
    {
        OnServerDisconnect();
    }

}
