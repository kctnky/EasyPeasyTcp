using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ClientMessageReceivedEventHandler(object sender, ClientMessageReceivedEventArgs e);

    public class ClientMessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
        public byte[] ReceivedBytes { get; internal set; }
    }
}
