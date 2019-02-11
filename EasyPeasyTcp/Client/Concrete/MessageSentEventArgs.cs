using System;
using System.Net;

namespace EasyPeasyTcp.Client
{
    public delegate void MessageSentEventHandler(object sender, MessageSentEventArgs e);

    public class MessageSentEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
        public byte[] SentBytes { get; internal set; }
    }
}
