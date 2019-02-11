using System;
using System.Net;

namespace EasyPeasyTcp.Client
{
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    public class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
        public byte[] ReceivedBytes { get; internal set; }
    }
}
