/****************************************************
	文件：NetManager.cs
	作者：JiahaoWu
	邮箱: jiahaodev@163.ccom
	日期：2020/02/07 22:06   	
	功能：网络通信管理者
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;


public static class NetManager
{
    #region 网络通信相关字段定义
    //套接字
    private static Socket socket;
    //接收缓冲区
    private static ByteArray readBuff;
    //写入队列
    private static Queue<ByteArray> writeQueue;
    //是否正在连接
    private static bool isConnecting = false;
    //是否正在关闭
    private static bool isClosing = false;
    //消息列表
    private static List<MsgBase> msgList = new List<MsgBase>();
    //消息列表长度
    private static int msgCount = 0;
    //每一次Update处理的“最大”消息量
    private static readonly int MAX_MESSAGE_FIRE = 10;
    #endregion

    #region 心跳机制相关字段定义
    //是否启用心跳机制
    public static bool isUsePing = true;
    //心跳时间间隔
    public static int pingInterval = 30;
    //上一次发送Ping的时间
    private static float lastPingTime = 0;
    //上次发送Pong的时间
    private static float lastPongTime = 0;
    #endregion

    #region Socket 连接、关闭监听
    //系统类型 网络事件
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    //事件委托类型
    public delegate void EventListener(string err);
    //事件监听列表
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    //添加事件监听
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    //删除事件监听
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
        }
    }

    //分发事件
    private static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    #endregion



    #region 消息协议监听
    //消息委托类型
    public delegate void MsgListener(MsgBase msgBase);
    //消息监听列表
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    //添加消息监听
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        //添加
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        //新增
        else
        {
            msgListeners[msgName] = listener;
        }
    }

    //删除消息监听
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
        }
    }

    //分发消息
    private static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }
    #endregion



    #region socket 连接、关闭 处理
    //Socket连接
    public static void Connect(string ip, int port)
    {
        //状态判断
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect fail, already connected!");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect fail, isConnecting");
            return;
        }
        //初始化成员
        InitState();
        //参数设置
        socket.NoDelay = true;
        //Connect
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    //初始化状态
    private static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        msgList = new List<MsgBase>();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        //监听Pong协议
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    //Connect回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
            //开始接收数据
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (Exception e)
        {
            Debug.Log("Socket Connect fail " + e.ToString());
            FireEvent(NetEvent.ConnectFail, e.ToString());
            isConnecting = false;
        }
    }

    //关闭连接
    public static void Close()
    {
        if (socket == null || socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }

        //还有数据在发送
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }

    #endregion



    #region 接收、发送数据处理
    //发送数据
    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }
        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        //重点
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }


    //发送数据回调
    private static void SendCallback(IAsyncResult ar)
    {
        //获取state、EndSend的处理
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }
        //完整性发送
        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();   //队首消息发送完毕，出队
                ba = writeQueue.First();//再次获取队首消息
            }
        }
        //继续发送
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }
        else if (isClosing)
        {
            socket.Close();
        }
    }


    //接收数据回调
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            //数据已经接收到bytes中，此处是更新索引
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
            if (readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.Resize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (Exception e)
        {
            Debug.Log("Socket Receive fail" + e.ToString());
        }
    }

    //对接收数据进行处理
    public static void OnReceiveData()
    {
        if (readBuff.length <= 2)
        {
            return;
        }
        //获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.length < bodyLength)
            return;
        readBuff.readIdx += 2;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();

        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
            msgCount++;
        }
        //继续读取消息
        if (readBuff.length > 2)
        {
            OnReceiveData();
        }
    }

    #endregion


    #region 循环更新接收到的消息
    //更新消息处理
    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    //更新消息
    public static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            else
            {
                break;
            }
        }
    }

    //发送Ping协议
    private static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }
        //发送Ping
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }
        //检测Pong时间
        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Close();
        }
    }

    //监听Pong协议
    private static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }
    #endregion

}

