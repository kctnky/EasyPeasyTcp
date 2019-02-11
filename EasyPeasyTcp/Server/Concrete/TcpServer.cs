using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EasyPeasyTcp.Server
{
    public class TcpServer : ITcpServer
    {
        #region Fields

        private ITcpServerConfig config;
        private List<IClientHandler> clients;
        private IPEndPoint listeningPoint;
        private Socket listenerSocket;       
        private Thread tListening;
        private Thread tCleaning;
        private bool isRunning = false;
        private bool isDisposed = false;

        #endregion

        #region Constructers

        public TcpServer()
        {
            clients = new List<IClientHandler>();
        }

        #endregion

        #region Finalizer

        ~TcpServer()
        {
            Dispose(false);
        }

        #endregion

        #region Events

        public event ServerStartedEventHandler ServerStarted;
        public event ServerStoppedEventHandler ServerStopped;
        public event ServerErrorOccuredEventHandler ServerErrorOccured;
        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event ClientMessageReceivedEventHandler ClientMessageReceived;
        public event ClientMessageSentEventHandler ClientMessageSent;
        public event ClientErrorOccuredEventHandler ClientErrorOccured;

        #endregion

        #region Properties

        public ITcpServerConfig Config
        {
            get { return config; }
        }

        public List<IPEndPoint> LocalEndPoints
        {
            get
            {
                List<IPEndPoint> list = new List<IPEndPoint>();
                foreach (ClientHandler client in clients)
                    list.Add(client.Socket.LocalEndPoint as IPEndPoint);
                return list;
            }
        }

        public List<IPEndPoint> RemoteEndPoints
        {
            get
            {
                List<IPEndPoint> list = new List<IPEndPoint>();
                foreach (ClientHandler client in clients)
                    list.Add(client.Socket.RemoteEndPoint as IPEndPoint);
                return list;
            }
        }

        public IPEndPoint ListeningPoint
        {
            get { return listeningPoint; }
        }

        public Socket ListenerSocket
        {
            get { return listenerSocket; }
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }

        #endregion

        #region Methods

        public void Start(ITcpServerConfig configuration)
        {
            try
            {
                if (isRunning)
                    throw new Exception("Server already started");

                isDisposed = false;
                config = configuration;

                listeningPoint = new IPEndPoint(config.IPAddress, config.Port);
                listenerSocket = new Socket(listeningPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenerSocket.Bind(listeningPoint);
                listenerSocket.Listen(128);

                isRunning = true;                

                tListening = new Thread(new ThreadStart(Listener));
                tListening.Priority = ThreadPriority.Normal;
                tListening.Start();

                tCleaning = new Thread(new ThreadStart(Cleaner));
                tCleaning.Priority = ThreadPriority.Normal;
                tCleaning.Start();

                RaiseServerStartedEvent(new ServerStartedEventArgs()
                {
                    ListeningPoint = listeningPoint
                });
            }
            catch (Exception e)
            {
                string errorMessage = "An error occured while starting server: " + e.Message;
                RaiseServerErrorOccuredEvent(new ServerErrorOccuredEventArgs()
                {
                    ListeningPoint = listeningPoint,
                    ErrorMessage = errorMessage
                });
            }
        }

        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;

            if (listenerSocket != null)
                listenerSocket.Close();

            if (tListening != null && tListening.IsAlive)
            {
                tListening.Join(1000);
                if (tListening.IsAlive)
                    tListening.Abort();
            }

            if (tCleaning != null && tCleaning.IsAlive)
            {
                tCleaning.Join(1000);
                if (tCleaning.IsAlive)
                    tCleaning.Abort();
            }

            RaiseServerStoppedEvent(new ServerStoppedEventArgs()
            {
                ListeningPoint = listeningPoint
            });

            foreach (IClientHandler client in clients)
                client.Dispose();

            clients.Clear();
        }

        public void Send(IPEndPoint remoteEndPoint, byte[] messageBytes)
        {
            try
            {
                IClientHandler client = FindClientHandler(remoteEndPoint);                
                client.Send(messageBytes);
            }
            catch (Exception e)
            {
                string errorMessage = "An error occured while sending message to " + remoteEndPoint.Address.ToString() + ": " + e.Message;
                RaiseServerErrorOccuredEvent(new ServerErrorOccuredEventArgs()
                {
                    ListeningPoint = listeningPoint,
                    ErrorMessage = errorMessage
                });
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Listener()
        {
            while (isRunning)
            {
                try
                {
                    Socket socket = listenerSocket.Accept();
                    socket.SendBufferSize = config.SendBufferSize;
                    socket.ReceiveBufferSize = config.ReceiveBufferSize;

                    IClientHandler client = new ClientHandler();
                    client.Connected = new Action<ClientConnectedEventArgs>(RaiseClientConnectedEvent);
                    client.Disconnected = new Action<ClientDisconnectedEventArgs>(RaiseClientDisconnectedEvent);
                    client.MessageReceived = new Action<ClientMessageReceivedEventArgs>(RaiseClientMessageReceivedEvent);
                    client.MessageSent = new Action<ClientMessageSentEventArgs>(RaiseClientMessageSentEvent);
                    client.ErrorOccured = new Action<ClientErrorOccuredEventArgs>(RaiseClientErrorOccuredEvent);
                    client.Start(this, socket);

                    lock (clients)
                        clients.Add(client);
                }
                catch (Exception e)
                {
                    if (isRunning)
                    {
                        string errorMessage = "An error occured while accepting new connections: " + e.Message;
                        RaiseServerErrorOccuredEvent(new ServerErrorOccuredEventArgs()
                        {
                            ListeningPoint = listeningPoint,
                            ErrorMessage = errorMessage
                        });
                    }
                }
            }
        }

        private void Cleaner()
        {
            while (isRunning)
            {
                DateTime T1 = DateTime.Now.ToUniversalTime();
                try
                {
                    List<IClientHandler> deleteList = new List<IClientHandler>();
                    lock (clients)
                    {
                        foreach (IClientHandler client in clients)
                        {
                            if (client.IsDead)
                            {
                                deleteList.Add(client);
                                client.Dispose();
                            }
                        }
                        foreach (IClientHandler client in deleteList)
                            clients.Remove(client);
                    }
                    deleteList.Clear();
                    deleteList = null;
                }
                catch (Exception e)
                {
                    if (isRunning)
                    {
                        string errorMessage = "An error occured while cleaning up client resources: " + e.Message;
                        RaiseServerErrorOccuredEvent(new ServerErrorOccuredEventArgs()
                        {
                            ListeningPoint = listeningPoint,
                            ErrorMessage = errorMessage
                        });
                    }
                }
                DateTime T2 = DateTime.Now.ToUniversalTime();

                int waitTime = config.CleanUpPeriod - (int)(T2 - T1).TotalMilliseconds;
                Thread.Sleep(waitTime < 0 ? 0 : waitTime);
            }
        }

        private IClientHandler FindClientHandler(IPEndPoint remoteEndPoint)
        {
            foreach (IClientHandler client in clients)
                if ((client.Socket.RemoteEndPoint as IPEndPoint) == remoteEndPoint)
                    return client;
            throw new Exception("Remote endpoint not found");
        }

        private void RaiseServerStartedEvent(ServerStartedEventArgs e)
        {
            ServerStarted?.Invoke(this, e);
        }

        private void RaiseServerStoppedEvent(ServerStoppedEventArgs e)
        {
            ServerStopped?.Invoke(this, e);
        }

        private void RaiseServerErrorOccuredEvent(ServerErrorOccuredEventArgs e)
        {
            ServerErrorOccured?.Invoke(this, e);
        }

        private void RaiseClientConnectedEvent(ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        private void RaiseClientDisconnectedEvent(ClientDisconnectedEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        private void RaiseClientMessageReceivedEvent(ClientMessageReceivedEventArgs e)
        {
            ClientMessageReceived?.Invoke(this, e);
        }

        private void RaiseClientMessageSentEvent(ClientMessageSentEventArgs e)
        {
            ClientMessageSent?.Invoke(this, e);
        }

        private void RaiseClientErrorOccuredEvent(ClientErrorOccuredEventArgs e)
        {
            ClientErrorOccured?.Invoke(this, e);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            Stop();

            if (disposing)
            {
                config = null;
                listenerSocket = null;
                tListening = null;
                tCleaning = null;
            }

            isDisposed = true;
        }

        #endregion
    }
}
