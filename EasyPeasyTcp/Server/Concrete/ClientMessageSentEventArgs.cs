using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ClientMessageSentEventHandler(object sender, ClientMessageSentEventArgs e);

    public class ClientMessageSentEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
        public byte[] SentBytes { get; internal set; }
    }
}
