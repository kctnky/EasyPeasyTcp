using System;
using System.Net;

namespace EasyPeasyTcp.Client
{
    public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);

    public class DisconnectedEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
    }
}
