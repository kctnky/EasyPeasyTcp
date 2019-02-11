using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EasyPeasyTcp.Server
{
    internal class ClientHandler : IClientHandler
    {
        #region Constant Fields

        private const byte STX = 0x02;
        private const byte EOT = 0x04;
        private const byte ESC = 0x1B;

        #endregion

        #region Fields

        private Thread tReceiving;
        private List<byte> messageBytesBuffer;
        private bool isMessage = false;
        private bool isEscaped = false;
        private Timer recvTimer;
        private DateTime prevReceivedTime;
        private DateTime lastReceivedTime;
        private bool isRunning = false;
        private bool isDisposed = false;

        #endregion

        #region Constructers

        public ClientHandler()
        {
            messageBytesBuffer = new List<byte>();
        }

        #endregion

        #region Finalizer

        ~ClientHandler()
        {
            Dispose(false);
        }

        #endregion

        #region Delegates

        public Action<ClientConnectedEventArgs> Connected { private get; set; }
        public Action<ClientDisconnectedEventArgs> Disconnected { private get; set; }
        public Action<ClientMessageReceivedEventArgs> MessageReceived { private get; set; }
        public Action<ClientMessageSentEventArgs> MessageSent { private get; set; }
        public Action<ClientErrorOccuredEventArgs> ErrorOccured { private get; set; }

        #endregion

        #region Properties

        public ITcpServer Server { get; private set; }

        public Socket Socket { get; private set; }

        public IPEndPoint LocalEndPoint { get; private set; }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public bool IsDead { get; private set; } = false;

        #endregion

        #region Methods

        public void Start(ITcpServer server, Socket socket)
        {
            if (isRunning)
                throw new Exception("Client handler already started");

            isDisposed = false;

            Server = server;
            Socket = socket;
            LocalEndPoint = Socket.LocalEndPoint as IPEndPoint;
            RemoteEndPoint = Socket.RemoteEndPoint as IPEndPoint;

            isRunning = true;

            tReceiving = new Thread(new ThreadStart(Receiver));
            tReceiving.Priority = ThreadPriority.Normal;
            tReceiving.Start();

            Connected?.Invoke(new ClientConnectedEventArgs()
            {
                LocalEndPoint = LocalEndPoint,
                RemoteEndPoint = RemoteEndPoint
            });
        }

        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;

            if (Socket != null)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }

            if (tReceiving != null && tReceiving.IsAlive)
            {
                tReceiving.Join(1000);
                if (tReceiving.IsAlive)
                    tReceiving.Abort();
            }

            if (recvTimer != null)
                recvTimer.Change(Timeout.Infinite, Timeout.Infinite);

            messageBytesBuffer.Clear();

            Disconnected?.Invoke(new ClientDisconnectedEventArgs()
            {
                LocalEndPoint = LocalEndPoint,
                RemoteEndPoint = RemoteEndPoint
            });
        }

        public void Send(byte[] messageBytes)
        {
            if (messageBytes.Length > Server.Config.MaxMessageLength)
                throw new Exception("Maximum message length is reached");

            byte[] buffer = new byte[Server.Config.SendBufferSize];
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

            MessageSent?.Invoke(new ClientMessageSentEventArgs()
            {
                LocalEndPoint = LocalEndPoint,
                RemoteEndPoint = RemoteEndPoint,
                SentBytes = messageBytes
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Receiver()
        {
            byte[] buffer = new byte[Server.Config.ReceiveBufferSize];

            prevReceivedTime = DateTime.Now.ToUniversalTime();
            lastReceivedTime = DateTime.Now.ToUniversalTime();
            recvTimer = new Timer(new TimerCallback(RecvTimerCallback), null, Server.Config.NoMessageTimeout, Server.Config.NoMessageTimeout);

            while (isRunning)
            {
                try
                {
                    int size = Socket.Receive(buffer);
                    if (size > 0)
                    {
                        lastReceivedTime = DateTime.Now.ToUniversalTime();
                        NewBytesReceived(buffer, size);
                    }
                    else
                        throw new Exception("Size 0 received");
                }
                catch (Exception e)
                {
                    if (isRunning)
                    {
                        string errorMessage = "An error occured while receiving new message: " + e.Message;
                        ErrorOccured?.Invoke(new ClientErrorOccuredEventArgs()
                        {
                            LocalEndPoint = LocalEndPoint,
                            RemoteEndPoint = RemoteEndPoint,
                            ErrorMessage = errorMessage
                        });
                        break;
                    }
                }
            }
        }

        private void RecvTimerCallback(object o)
        {
            if (lastReceivedTime.Equals(prevReceivedTime))
            {
                IsDead = true;
                recvTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
                prevReceivedTime = lastReceivedTime;
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
                                MessageReceived?.Invoke(new ClientMessageReceivedEventArgs()
                                {
                                    LocalEndPoint = LocalEndPoint,
                                    RemoteEndPoint = RemoteEndPoint,
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

                    if (messageBytesBuffer.Count > Server.Config.MaxMessageLength)
                        throw new Exception("Maximum message length is reached");
                }
            }
            catch (Exception e)
            {
                isMessage = false;
                isEscaped = false;
                messageBytesBuffer.Clear();
                string errorMessage = "An error occured while framing received bytes: " + e.Message;
                ErrorOccured?.Invoke(new ClientErrorOccuredEventArgs()
                {
                    LocalEndPoint = LocalEndPoint,
                    RemoteEndPoint = RemoteEndPoint,
                    ErrorMessage = errorMessage
                });
            }
        }

        private void SendBytes(byte[] buffer, int size)
        {
            int startingOffset = 0;
            while (startingOffset < size)
            {
                int sentBytes = Socket.Send(buffer, startingOffset, size - startingOffset, SocketFlags.None);
                startingOffset += sentBytes;
            }
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            Stop();

            if (disposing)
            {
                Server = null;
                Socket = null;
                tReceiving = null;
                recvTimer = null;
            }

            isDisposed = true;
        }
        
        #endregion
    }
}
