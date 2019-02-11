using System;
using System.Net;

namespace EasyPeasyTcp.Client
{
    public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs e);

    public class ConnectedEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
    }
}
