using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ClientDisconnectedEventHandler(object sender, ClientDisconnectedEventArgs e);

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
    }
}
