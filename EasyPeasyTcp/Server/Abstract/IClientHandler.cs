using System;
using System.Net;
using System.Net.Sockets;

namespace EasyPeasyTcp.Server
{
    internal interface IClientHandler : IDisposable
    {
        Action<ClientConnectedEventArgs> Connected { set; }
        Action<ClientDisconnectedEventArgs> Disconnected { set; }
        Action<ClientMessageReceivedEventArgs> MessageReceived { set; }
        Action<ClientMessageSentEventArgs> MessageSent { set; }
        Action<ClientErrorOccuredEventArgs> ErrorOccured { set; }

        ITcpServer Server { get; }
        Socket Socket { get; }
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        bool IsDead { get; }

        void Start(ITcpServer server, Socket socket);
        void Stop();
        void Send(byte[] messageBytes);
    }
}
