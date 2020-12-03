using Google.Protobuf;
using HxpTest.AddressBook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using static HxpTest.AddressBook.Person.Types;

namespace UnityServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AddListeners();
            //Console.WriteLine("Hello World!");

            //创建一个新的Socket,这里我们使用最常用的基于TCP的Stream Socket（流式套接字）
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //将该socket绑定到主机上面的某个端口
            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.bind.aspx
            socket.Bind(new IPEndPoint(IPAddress.Any, 4530));

            //启动监听，并且设置一个最大的队列长度
            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.listen(v=VS.100).aspx
            socket.Listen(100);



            //开始接受客户端连接请求 异步方式，不阻塞
            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginaccept.aspx
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);

            Console.WriteLine("Server is ready!");
            Console.Read();

        }

        public static void AddListeners()
        {
           NetMsg.AddListener(102, new NetHandler(Test));

        }

        public static void Test(byte[] data, ushort msgId)
        {
            Person person = Person.Parser.ParseFrom(data);
            Console.WriteLine($"服务端收到客户端的消息{msgId} 回调{person.Name}");
        }

        public static void ClientAccepted(IAsyncResult ar)
        {

            var socket = ar.AsyncState as Socket;

            //这就是客户端的Socket实例，我们后续可以将其保存起来
            var client = socket.EndAccept(ar);
            Console.WriteLine($"有新的客户端连接: {client.RemoteEndPoint}");

            Person john = new Person
            {
                Id = 1234,
                Name = "Hi there, I accept you request at " + DateTime.Now.ToString(),
                Email = "jdoe@example.com",
                Phones = { new Person.Types.PhoneNumber { Number = "555-4321", Type = PhoneType.Home } }
            };

           

            //byte[] byteArray = john.ToByteArray();

            //byte[] byteArray = ProtoBufUtil.Encode(john.ToByteArray(), 100);


            //给客户端发送一个欢迎消息
            //client.Send(byteArray);
            NetMsg.SendMsg(client, john, 101);


            //测试代码
            //实现每隔两秒钟给服务器发一个消息
            //这里我们使用了一个定时器
            var timer = new System.Timers.Timer();
            timer.Interval = 2000D;
            timer.Enabled = true;
            timer.Elapsed += (o, a) =>
            {
                //检测客户端Socket的状态
                if (client.Connected)
                {
                    try
                    {
                        //使用protobuf
                        Person johnSC = new Person
                        {
                            Id = 1234,
                            Name = "Message from server at " + DateTime.Now.ToString(),
                            Email = "jdoe@example.com",
                            Phones = { new Person.Types.PhoneNumber { Number = "555-4321", Type = PhoneType.Home } }
                        };

                        //byte[] byteArraySC = johnSC.ToByteArray();
                        //byte[] byteArraySC = ProtoBufUtil.Encode(johnSC.ToByteArray(), 101);
                        //client.Send(byteArraySC);

                        NetMsg.SendMsg(client, johnSC, 101);

                        //client.Send(Encoding.Unicode.GetBytes("Message from server at " + DateTime.Now.ToString()));
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    timer.Stop();
                    timer.Enabled = false;
                    Console.WriteLine("Client is disconnected, the timer is stop.");
                }
            };
            timer.Start();


            //接收客户端的消息(这个和在客户端实现的方式是一样的）
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);

            //准备接受下一个客户端请求
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        static byte[] buffer = new byte[1024];

        public static void ReceiveMessage(IAsyncResult ar)
        {
            var clientSocket = ar.AsyncState as Socket;
            try
            {
   
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                var length = clientSocket.EndReceive(ar);
                if(length > 0)
                {
                    //读取出来消息内容 使用protobuf
                    var data = buffer.Take(length).ToArray();
                    //var message = Person.Parser.ParseFrom(data);
                    //ushort msgId = 0;
                    //Person message = ProtoBufUtil.Uncode<Person>(data, out msgId);

                    //NetMsg.HandleMsg<Person>(data);

                    MemoryStream ms = null;
                    using (ms = new MemoryStream(data))
                    {
                        BinaryReader reader = new BinaryReader(ms);
                        ushort msgLen = reader.ReadUInt16();
                        ushort protoId = reader.ReadUInt16();

                        Console.WriteLine($"[Server] receive ：protoID：{protoId}，dataLen：{msgLen}");
                        if (msgLen <= data.Length - 4)
                        {
                            byte[] pbdata = reader.ReadBytes(msgLen);
                            NetMsg.HandleMsg(pbdata, protoId);
                        }
                        else
                        {
                            Console.WriteLine($"[Server] {protoId} 协议长度错误");
                        }
                    }

                    //var message = Encoding.Unicode.GetString(buffer, 0, length);
                    //显示消息
                    //Console.WriteLine(message.Name);

                    //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                    clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), clientSocket);

                }
                else
                {
                    Console.WriteLine($"客户端断开连接：{clientSocket.RemoteEndPoint}");
                    OnClientDisconnect(clientSocket);
                }
                
            }
            catch (SocketException ex)
            {
                // 如果socket有未发送完的数据，断开时会触发10054异常，在此捕捉
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Console.WriteLine($"clientSocket有未发送完的数据 {ex.Message}");
                    OnClientDisconnect(clientSocket);
                }
                    
            }
        }

        private static void OnClientDisconnect(Socket clientSocket)
        {
            clientSocket.Close();
        }
    }

   
}
