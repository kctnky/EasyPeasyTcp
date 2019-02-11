using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ClientConnectedEventHandler(object sender, ClientConnectedEventArgs e);

    public class ClientConnectedEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
    }
}
