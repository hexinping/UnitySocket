﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UClient : MonoBehaviour
{

    private static byte[] buffer = new byte[1024];
    private Socket clientSocket;

    private float sendTimeIntveral = 2;
    private float lastSendTime;
    // Start is called before the first frame update
    void Start()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket = socket;
        //连接到指定服务器的指定端口
        //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.connect.aspx
        socket.Connect("127.0.0.1", 4530);


        //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginreceive.aspx
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);

        Debug.Log("connect to the server");

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
                var message = Encoding.Unicode.GetString(buffer, 0, length);
                //显示消息
                Debug.Log(message);

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
        //测试code
        if (Time.time - lastSendTime >= sendTimeIntveral)
        {
            lastSendTime = Time.time;
            var message = "Message from client : " + DateTime.Now;
            var outputBuffer = Encoding.Unicode.GetBytes(message);
            clientSocket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
        }
    }
    private void OnServerDisconnect()
    {
        clientSocket.Close();
    }

    private void OnDestroy()
    {
        OnServerDisconnect();
    }
}