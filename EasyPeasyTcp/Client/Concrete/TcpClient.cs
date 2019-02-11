using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPeasyTcp.Client
{
    public class TcpClient : ITcpClient
    {
        #region Constant Fields

        private const byte STX = 0x02;
        private const byte EOT = 0x04;
        private const byte ESC = 0x1B;

        #endregion

        #region Fields

        private ITcpClientConfig config;
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        private Socket socket;
        private Thread tReceiving;
        private List<byte> messageBytesBuffer;
        private bool isMessage = false;
        private bool isEscaped = false;
        private bool isConnected = false;
        private bool isDisposed = false;

        #endregion

        #region Constructers

        public TcpClient()
        {
            messageBytesBuffer = new List<byte>();
        }

        #endregion

        #region Finalizer

        ~TcpClient()
        {
            Dispose(false);
        }

        #endregion

        #region Events

        public event ConnectedEventHandler Connected;
        public event DisconnectedEventHandler Disconnected;
        public event MessageReceivedEventHandler MessageReceived;
        public event MessageSentEventHandler MessageSent;
        public event ErrorOccuredEventHandler ErrorOccured;

        #endregion

        #region Properties

        public ITcpClientConfig Config
        {
            get { return config; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return localEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return remoteEndPoint; }
        }

        public Socket Socket
        {
            get { return socket; }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }

        #endregion

        #region Methods

        public void Connect(ITcpClientConfig configuration)
        {
            try
            {
                if (isConnected)
                    throw new Exception("Client already connected");

                isDisposed = false;
                config = configuration;

                remoteEndPoint = new IPEndPoint(config.ServerIPAddress, config.ServerPort);
                socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(remoteEndPoint);

                isConnected = true;
                localEndPoint = socket.LocalEndPoint as IPEndPoint;

                socket.SendBufferSize = config.SendBufferSize;
                socket.ReceiveBufferSize = config.ReceiveBufferSize;

                tReceiving = new Thread(new ThreadStart(Receiver));
                tReceiving.Priority = ThreadPriority.Normal;
                tReceiving.Start();

                RaiseConnectedEvent(new ConnectedEventArgs()
                {
                    LocalEndPoint = localEndPoint,
                    RemoteEndPoint = remoteEndPoint
                });
            }
            catch (Exception e)
            {
                string errorMessage = "An error occured while connecting to server: " + e.Message;
                RaiseErrorEvent(new ErrorOccuredEventArgs()
                {
                    LocalEndPoint = localEndPoint,
                    RemoteEndPoint = remoteEndPoint,
                    ErrorMessage = errorMessage
                });
            }
        }

        public void Disconnect()
        {
            if (!isConnected)
                return;

            isConnected = false;

            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            if (tReceiving != null && tReceiving.IsAlive)
            {
                tReceiving.Join(1000);
                if (tReceiving.IsAlive)
                    tReceiving.Abort();
            }

            messageBytesBuffer.Clear();

            RaiseDisconnectedEvent(new DisconnectedEventArgs()
            {
                LocalEndPoint = localEndPoint,
                RemoteEndPoint = remoteEndPoint
            });
        }

        public void Send(byte[] messageBytes)
        {
            try
            {
                if (messageBytes.Length > config.MaxMessageLength)
                    throw new Exception("Maximum message length is reached");

                byte[] buffer = new byte[config.SendBufferSize];
                int bufferIndex = 0;

                buffer[bufferIndex++] = STX;

                for (int i = 0; i < messageBytes.Length; i++)
                {
                    if (messageBytes[i] == STX || messageBytes[i] == EOT || messageBytes[i] == ESC)
                    {
                        if (bufferIndex == buffer.Length - 1)
                        {
                            SendBytes(buffer, bufferIndex);
                            bufferIndex = 0;
                        }

                        buffer[bufferIndex++] = ESC;
                        buffer[bufferIndex++] = messageBytes[i];
                        if (bufferIndex == buffer.Length)
                        {
                            SendBytes(buffer, bufferIndex);
                            bufferIndex = 0;
                        }
                    }
                    else
                    {
                        buffer[bufferIndex++] = messageBytes[i];
                        if (bufferIndex == buffer.Length)
                        {
                            SendBytes(buffer, bufferIndex);
                            bufferIndex = 0;
                        }
                    }
                }

                buffer[bufferIndex++] = EOT;
                SendBytes(buffer, bufferIndex);

                RaiseMessageSentEvent(new MessageSentEventArgs()
                {
                    LocalEndPoint = localEndPoint,
                    RemoteEndPoint = remoteEndPoint,
                    SentBytes = messageBytes
                });
            }
            catch (Exception e)
            {
                string errorMessage = "An error occured while sending message: " + e.Message;
                RaiseErrorEvent(new ErrorOccuredEventArgs()
                {
                    LocalEndPoint = localEndPoint,
                    RemoteEndPoint = remoteEndPoint,
                    ErrorMessage = errorMessage
                });
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Receiver()
        {
            byte[] buffer = new byte[config.ReceiveBufferSize];

            while (isConnected)
            {
                try
                {
                    int size = socket.Receive(buffer);
                    if (size > 0)
                        NewBytesReceived(buffer, size);
                    else
                        throw new Exception("Size 0 received");
                }
                catch (Exception e)
                {
                    if (isConnected)
                    {
                        string errorMessage = "An error occured while receiving new message: " + e.Message;
                        RaiseErrorEvent(new ErrorOccuredEventArgs()
                        {
                            LocalEndPoint = localEndPoint,
                            RemoteEndPoint = remoteEndPoint,
                            ErrorMessage = errorMessage
                        });
                        break;
                    }
                }
            }

            Task.Delay(new TimeSpan(0, 0, 0, 0, 1)).ContinueWith(o => { Disconnect(); });
        }

        private void NewBytesReceived(byte[] received, int size)
        {
            int index = 0;
            byte current;

            try
            {
                while (true)
                {
                    current = received[index++];

                    if (!isMessage)
                    {
                        if (current == STX)
                            isMessage = true;
                    }
                    else
                    {
                        if (!isEscaped)
                        {
                            if (current == EOT)
                            {
                                RaiseMessageReceivedEvent(new MessageReceivedEventArgs()
                                {
                                    LocalEndPoint = localEndPoint,
                                    RemoteEndPoint = remoteEndPoint,
                                    ReceivedBytes = messageBytesBuffer.ToArray()
                                });
                                messageBytesBuffer.Clear();
                                isMessage = false;
                            }
                            else if (current == ESC)
                                isEscaped = true;
                            else
                                messageBytesBuffer.Add(current);
                        }
                        else
                        {
                            if (current == ESC || current == STX || current == EOT)
                                messageBytesBuffer.Add(current);
                            else
                                throw new Exception("Unexpected character after Escape in message content");
                            isEscaped = false;
                        }
                    }

                    if (index == size)
                        break;

                    if (messageBytesBuffer.Count > config.MaxMessageLength)
                        throw new Exception("Maximum message length is reached");
                }
            }
            catch (Exception e)
            {
                isMessage = false;
                isEscaped = false;
                messageBytesBuffer.Clear();
                string errorMessage = "An error occured while framing received bytes: " + e.Message;
                RaiseErrorEvent(new ErrorOccuredEventArgs()
                {
                    LocalEndPoint = localEndPoint,
                    RemoteEndPoint = remoteEndPoint,
                    ErrorMessage = errorMessage
                });
            }
        }

        private void SendBytes(byte[] buffer, int size)
        {            
            int startingOffset = 0;
            while (startingOffset < size)
            {
                int sentBytes = socket.Send(buffer, startingOffset, size - startingOffset, SocketFlags.None);
                startingOffset += sentBytes; 
            }
        }

        private void RaiseConnectedEvent(ConnectedEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        private void RaiseDisconnectedEvent(DisconnectedEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        private void RaiseMessageReceivedEvent(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void RaiseMessageSentEvent(MessageSentEventArgs e)
        {
            MessageSent?.Invoke(this, e);
        }

        private void RaiseErrorEvent(ErrorOccuredEventArgs e)
        {
            ErrorOccured?.Invoke(this, e);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            Disconnect();

            if (disposing)
            {
                config = null;
                socket = null;
                tReceiving = null;
            }

            isDisposed = true;
        }

        #endregion
    }
}
