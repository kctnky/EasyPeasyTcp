using System;
using System.Net;
using System.Net.Sockets;

namespace EasyPeasyTcp.Client
{
    public interface ITcpClient : IDisposable
    {
        event ConnectedEventHandler Connected;
        event DisconnectedEventHandler Disconnected;
        event MessageReceivedEventHandler MessageReceived;
        event MessageSentEventHandler MessageSent;
        event ErrorOccuredEventHandler ErrorOccured;

        ITcpClientConfig Config { get; }
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        Socket Socket { get; }
        bool IsConnected { get; }

        void Connect(ITcpClientConfig config);
        void Disconnect();
        void Send(byte[] messageBytes);
    }
}
