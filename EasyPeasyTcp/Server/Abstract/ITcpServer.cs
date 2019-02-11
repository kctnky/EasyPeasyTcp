using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EasyPeasyTcp.Server
{
    public interface ITcpServer : IDisposable
    {
        event ServerStartedEventHandler ServerStarted;
        event ServerStoppedEventHandler ServerStopped;
        event ServerErrorOccuredEventHandler ServerErrorOccured;
        event ClientConnectedEventHandler ClientConnected;
        event ClientDisconnectedEventHandler ClientDisconnected;
        event ClientMessageReceivedEventHandler ClientMessageReceived;
        event ClientMessageSentEventHandler ClientMessageSent;
        event ClientErrorOccuredEventHandler ClientErrorOccured;

        ITcpServerConfig Config { get; }
        List<IPEndPoint> LocalEndPoints { get; }
        List<IPEndPoint> RemoteEndPoints { get; }
        IPEndPoint ListeningPoint { get; }
        Socket ListenerSocket { get; }
        bool IsRunning { get; }

        void Start(ITcpServerConfig config);
        void Stop();
        void Send(IPEndPoint remoteEndPoint, byte[] messageBytes);
    }
}
