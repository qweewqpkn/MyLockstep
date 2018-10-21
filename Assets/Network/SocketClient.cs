using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network
{
    class Message
    {
        public ServiceNo mID;
        public byte[] mData;
        public MemoryStream mMS = new MemoryStream();

        public Message(ServiceNo id, IMessage data)
        {
            mID = id;
            data.WriteTo(mMS);
            mData = mMS.ToArray();
        }
    }

    class MessageHandle
    {
        private List<Action<IMessage>> mList;
        private MessageParser mParser;

        public MessageHandle(MessageParser parser)
        {
            mList = new List<Action<IMessage>>();
            mParser = parser;
        }

        public void Add(Action<IMessage> handle)
        {
            mList.Add(handle);
        }

        public IMessage Parser(byte[] data)
        {
            return mParser.ParseFrom(data);
        }

        public void Handle(IMessage data)
        {
            for (int i = 0; i < mList.Count; i++)
            {
                mList[i](data);
            }
        }
    }

    class SocketClient
    {
        private Socket mSocket;
        private const int mReceiveBufferSize = 4096;
        private byte[] mReceiveBuffer = new byte[mReceiveBufferSize];
        //缓存接收的数据，当数据达到一个消息的大小我们便解析，否则继续收取等待满足条件
        private MemoryStream mReceiveMS;
        private BinaryReader mReceiveReader;

        private Queue<Message> mMessageQueue = new Queue<Message>();
        private object mSendMessageLockObj = new object();

        private Queue<C2SPackage> mReceiveMessageQueue = new Queue<C2SPackage>();
        private object mReceiveMessageLockObj = new object();
        private ManualResetEvent mResetEvent = new ManualResetEvent(true);
        private MemoryStream mSendMS;
        private const int mSendBufferSize = 4096;
        private byte[] mSendBuffer = new byte[mSendBufferSize];

        private Dictionary<ServiceNo, MessageHandle> MessageHandleMap = new Dictionary<ServiceNo, MessageHandle>();

        private static SocketClient mInstance;
        public static SocketClient Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new SocketClient();
                }

                return mInstance;
            }
        }

        public SocketClient()
        {
            mReceiveMS = new MemoryStream();
            mReceiveReader = new BinaryReader(mReceiveMS);
            mSendMS = new MemoryStream();
        }

        public void Init()
        {
            IPAddress address = IPAddress.Parse("192.168.0.173"); //1.236
            IPEndPoint endpoint = new IPEndPoint(address, 8888);
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.BeginConnect(endpoint, OnConnect, mSocket);
            Thread sendThread = new Thread(new ThreadStart(SendThread));
            sendThread.Start();
        }

        void OnConnect(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                if (socket.Connected)
                {
                    mSocket.BeginReceive(mReceiveBuffer, 0, mReceiveBufferSize, 0, OnReceive, socket);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                Close();
            }
        }

        void OnReceive(IAsyncResult ar)
        {
            try
            {
                SocketError socketError;
                Socket socket = (Socket)ar.AsyncState;
                int receiveNum = socket.EndReceive(ar, out socketError);
                if (socketError == SocketError.Success && receiveNum > 0)
                {
                    mReceiveMS.Seek(0, SeekOrigin.End);
                    mReceiveMS.Write(mReceiveBuffer, 0, receiveNum);
                    mReceiveMS.Seek(0, SeekOrigin.Begin);
                    while (mReceiveMS.Length - mReceiveMS.Position > 4)
                    {
                        int messageLength = mReceiveReader.ReadInt32();
                        if (mReceiveMS.Length - mReceiveMS.Position >= messageLength)
                        {
                            byte[] packageData = mReceiveReader.ReadBytes(messageLength);
                            OnParseMessage(packageData);
                        }
                        else
                        {
                            mReceiveMS.Position = mReceiveMS.Position - 4;
                            break;
                        }
                    }

                    //之前解析完后，可能有多余消息的数据,要读取出来
                    byte[] leaveBytes = mReceiveReader.ReadBytes((int)(mReceiveMS.Length - mReceiveMS.Position));
                    mReceiveMS.Position = 0;
                    mReceiveMS.SetLength(0);
                    mReceiveMS.Write(leaveBytes, 0, leaveBytes.Length);

                    if (socket != null && socket.Connected)
                    {
                        socket.BeginReceive(mReceiveBuffer, 0, mReceiveBufferSize, 0, OnReceive, socket);
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                Close();
            }
        }

        void OnParseMessage(byte[] packageData)
        {
            C2SPackage package = C2SPackage.Parser.ParseFrom(packageData);
            mReceiveMessageQueue.Enqueue(package);
        }

        public void Update()
        {
            while(true)
            {
                lock(mReceiveMessageLockObj)
                {
                    if(mReceiveMessageQueue.Count > 0)
                    {
                        C2SPackage package = mReceiveMessageQueue.Dequeue();
                        MessageHandle messageHandle;
                        if (MessageHandleMap.TryGetValue(package.Id, out messageHandle))
                        {
                            IMessage messageData = messageHandle.Parser(package.Data.ToByteArray());
                            messageHandle.Handle(messageData);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public void SendData(ServiceNo id, IMessage data)
        {
            Message message = new Message(id, data);

            lock (mSendMessageLockObj)
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
                    lock (mSendMessageLockObj)
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

                    try
                    {
                        if (mSocket.Connected)
                        {
                            mSocket.Send(mSendBuffer, 0, packageLength + 4, 0);
                        }
                    }
                    catch (SocketException e)
                    {
                        Close();
                    }
                }

                mResetEvent.Reset();
                mResetEvent.WaitOne();
            }
        }

        public void RegisterMessage<T>(ServiceNo id, Action<T> action) where T : IMessage
        {
            MessageHandle handle;
            if (MessageHandleMap.TryGetValue(id, out handle))
            {

            }
            else
            {
                Type t = typeof(T);
                System.Reflection.PropertyInfo pi = t.GetProperty("Parser", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public);
                MessageParser parser = (MessageParser)pi.GetValue(null, null);
                handle = new MessageHandle(parser);
                MessageHandleMap[id] = handle;
            }

            if (handle != null)
            {
                Action<IMessage> baseAction = new Action<IMessage>(o =>
                {
                    action((T)o);
                });
                handle.Add(baseAction);
            }
        }

        public void Close()
        {
            if (mSocket != null && mSocket.Connected)
            {
                mSocket.Shutdown(SocketShutdown.Both);
            }

            mSocket.Close();
            mSocket = null;
        }
    }
}
