using Google.Protobuf;
using Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestServer
{
    class Message
    {
        public ServiceNo mID;
        public byte[] mData;
        public MemoryStream mMS = new MemoryStream();
        public List<string> mEndPointList;

        public Message(ServiceNo id, IMessage data, Socket socket)
        {
            mID = id;
            data.WriteTo(mMS);
            mData = mMS.ToArray();
            mEndPointList = new List<string>();
            if(socket != null)
            {
                mEndPointList.Add(socket.RemoteEndPoint.ToString());
            }
        }

        public Message(ServiceNo id, IMessage data, List<Socket> socketList)
        {
            mID = id;
            data.WriteTo(mMS);
            mData = mMS.ToArray();
            mEndPointList = new List<string>();
            if(socketList != null)
            {
                for (int i = 0; i < socketList.Count; i++)
                {
                    mEndPointList.Add(socketList[i].RemoteEndPoint.ToString());
                }
            }
        }
    }

    class MessageHandle
    {
        private List<Action<Socket, IMessage>> mList;
        private MessageParser mParser;

        public MessageHandle(MessageParser parser)
        {
            mList = new List<Action<Socket, IMessage>>();
            mParser = parser;
        }

        public void Add(Action<Socket, IMessage> handle)
        {
            mList.Add(handle);
        }

        public IMessage Parser(byte[] data)
        {
            return mParser.ParseFrom(data);
        }

        public void Handle(Socket socket, IMessage data)
        {
            for (int i = 0; i < mList.Count; i++)
            {
                mList[i](socket, data);
            }
        }
    }

    class ClientInfo
    {
        public Socket mSocket;
        public const int mBufferLength = 4096;
        public byte[] mReceiveBuffer = new byte[mBufferLength];
        public MemoryStream mReceiveMS = new MemoryStream();
        public BinaryReader mReceiveReader;

        public string SocketEndpoint
        {
            get
            {
                if(mSocket != null)
                {
                    return mSocket.RemoteEndPoint.ToString();
                }

                return "";
            }
        }

        public ClientInfo()
        {
            mReceiveReader = new BinaryReader(mReceiveMS);
        }
    }

    class SocketServer
    {
        private Socket mListenSocket;
        private ManualResetEvent mAcceptResetEvent = new ManualResetEvent(false);
        //保存连接的客户端信息
        private Dictionary<string, ClientInfo> mClientInfoMap = new Dictionary<string, ClientInfo>();

        private Queue<Message> mMessageQueue = new Queue<Message>();
        private object mMessageLockObj = new object();
        private ManualResetEvent mResetEvent = new ManualResetEvent(true);
        private MemoryStream mSendMS;
        private const int mSendBufferSize = 4096;
        private byte[] mSendBuffer = new byte[mSendBufferSize];

        private Dictionary<ServiceNo, MessageHandle> MessageHandleMap = new Dictionary<ServiceNo, MessageHandle>();

        private static SocketServer mInstance;
        public static SocketServer Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new SocketServer();
                }
                return mInstance;
            }
        }

        public SocketServer()
        {
            mSendMS = new MemoryStream();
        }

        public void Init()
        {
            Thread acceptThread = new Thread(new ThreadStart(AcceptThread));
            acceptThread.Start();

            Thread sendThread = new Thread(new ThreadStart(SendThread));
            sendThread.Start();
        }

        private void AcceptThread()
        {
            IPAddress address = IPAddress.Parse("192.168.0.173");//1.236
            IPEndPoint endpoint = new IPEndPoint(address, 8888);
            mListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mListenSocket.Bind(endpoint);
            mListenSocket.Listen(1);

            while(true)
            {
                mListenSocket.BeginAccept(OnAccept, mListenSocket);
                mAcceptResetEvent.Reset();
                mAcceptResetEvent.WaitOne(); 
            }
        }

        private void OnAccept(IAsyncResult ar)
        {
            mAcceptResetEvent.Set();

            Socket listenSocket = (Socket)ar.AsyncState;
            Socket socket = listenSocket.EndAccept(ar);

            string id = socket.RemoteEndPoint.ToString();
            ClientInfo ci = new ClientInfo();
            mClientInfoMap[id] = ci;
            ci.mSocket = socket;

            socket.BeginReceive(ci.mReceiveBuffer, 0, ClientInfo.mBufferLength, 0, OnReceive, ci);
        }

        public void OnReceive(IAsyncResult ar)
        {
            try
            {
                SocketError socketError;
                ClientInfo ci = (ClientInfo)ar.AsyncState;
                int length = ci.mSocket.EndReceive(ar, out socketError);
                if (socketError == SocketError.Success && length > 0)
                {
                    ci.mReceiveMS.Seek(0, SeekOrigin.End);
                    ci.mReceiveMS.Write(ci.mReceiveBuffer, 0, length);
                    ci.mReceiveMS.Seek(0, SeekOrigin.Begin);
                    while (ci.mReceiveMS.Length - ci.mReceiveMS.Position > 4)
                    {
                        int messageLength = ci.mReceiveReader.ReadInt32();
                        if (ci.mReceiveMS.Length - ci.mReceiveMS.Position >= messageLength)
                        {
                            //存在一個完整的消息
                            byte[] messageData = ci.mReceiveReader.ReadBytes(messageLength);
                            OnParseMessage(ci.mSocket, messageData);
                        }
                        else
                        {
                            //不存在一個完整的消息,将buffer指针回退4字节
                            ci.mReceiveMS.Position = ci.mReceiveMS.Position - 4;
                            break;
                        }
                    }

                    //之前解析完后，可能有多余消息的数据,要读取出来
                    byte[] leaveBytes = ci.mReceiveReader.ReadBytes((int)(ci.mReceiveMS.Length - ci.mReceiveMS.Position));
                    ci.mReceiveMS.Position = 0;
                    ci.mReceiveMS.SetLength(0);
                    ci.mReceiveMS.Write(leaveBytes, 0, leaveBytes.Length);

                    ci.mSocket.BeginReceive(ci.mReceiveBuffer, 0, ClientInfo.mBufferLength, 0, OnReceive, ci);
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.ToString());         
            }
        }

        void OnParseMessage(Socket socket, byte[] packageData)
        {
            C2SPackage package = C2SPackage.Parser.ParseFrom(packageData);

            MessageHandle messageHandle;
            if (MessageHandleMap.TryGetValue(package.Id, out messageHandle))
            {
                IMessage messageData = messageHandle.Parser(package.Data.ToByteArray());
                messageHandle.Handle(socket, messageData);
            }
        }

        public void RegisterMessage<T>(ServiceNo id, Action<Socket, T> action) where T : IMessage
        {
            MessageHandle handle;
            if (MessageHandleMap.TryGetValue(id, out handle))
            {

            }
            else
            {
                Type t = typeof(T);
                System.Reflection.PropertyInfo pi = t.GetProperty("Parser", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public);
                MessageParser parser = (MessageParser)pi.GetValue(null);
                handle = new MessageHandle(parser);
                MessageHandleMap[id] = handle;
            }

            if (handle != null)
            {
                Action<Socket, IMessage> baseAction = new Action<Socket, IMessage>((socket, message) =>
                {
                    action(socket, (T)message);
                });
                handle.Add(baseAction);
            }
        }

        public void SendMessage(ServiceNo id, IMessage data, Socket socket)
        {
            Message message = new Message(id, data, socket);
            lock (mMessageLockObj)
            {
                mMessageQueue.Enqueue(message);
            }
            mResetEvent.Set();
        }

        public void BroadcastMessage(ServiceNo id, IMessage data, List<Socket> socketList = null)
        {
            if(socketList == null)
            {
                socketList = new List<Socket>();
                foreach (var v in mClientInfoMap)
                {
                    socketList.Add(v.Value.mSocket);
                }
            }

            Message message = new Message(id, data, socketList);
            lock (mMessageLockObj)
            {
                mMessageQueue.Enqueue(message);
            }
            mResetEvent.Set();
        }

        void SendThread()
        {
            while (true)
            {
                while (true)
                {
                    Message message = null;
                    lock (mMessageLockObj)
                    {
                        if (mMessageQueue.Count <= 0)
                        {
                            break;
                        }
                        else
                        {
                            message = mMessageQueue.Dequeue();
                        }
                    }

                    //序列化消息
                    mSendMS.SetLength(0);
                    mSendMS.Position = 0;
                    C2SPackage package = new C2SPackage();
                    package.Id = message.mID;
                    package.Data = ByteString.CopyFrom(message.mData);
                    package.WriteTo(mSendMS);

                    //缓冲是否足够大
                    int packageLength = (int)mSendMS.Length;
                    while (packageLength > mSendBuffer.Length - 4)
                    {
                        mSendBuffer = new byte[mSendBuffer.Length * 2];
                    }

                    //长度加内容发送
                    Array.Copy(BitConverter.GetBytes(packageLength), 0, mSendBuffer, 0, 4);
                    Array.Copy(mSendMS.ToArray(), 0, mSendBuffer, 4, packageLength);

                    for(int i = 0; i < message.mEndPointList.Count; i++)
                    {
                        ClientInfo ci;
                        if(mClientInfoMap.TryGetValue(message.mEndPointList[i], out ci))
                        {
                            Socket socket = ci.mSocket;
                            if (IsSocketConnected(socket))
                            {
                                try
                                {
                                    socket.Send(mSendBuffer, 0, packageLength + 4, 0);
                                }
                                catch
                                {
                                    Console.WriteLine("remove");
                                    mClientInfoMap.Remove(message.mEndPointList[i]);
                                }
                            }
                            else
                            {
                                Console.WriteLine("remove");
                                mClientInfoMap.Remove(message.mEndPointList[i]);
                            }
                        }
                    }
                }

                mResetEvent.Reset();
                mResetEvent.WaitOne();
            }
        }

        public void Close(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }

            socket.Close();
            socket = null;
        }

        public bool IsSocketConnected(Socket socket)
        {
            bool rule1 = socket.Poll(1000, SelectMode.SelectRead);
            bool rule2 = socket.Available == 0;
            bool rule3 = socket.Connected;
            if((rule1 && rule2) || !rule3)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    class TestServer
    {
        static void Main(string[] args)
        {
            SocketServer.Instance.Init();
            ProxyManager.Instance.Init();

            while (true)
            {
                string content = Console.ReadLine();
            }
        }
    }
}
