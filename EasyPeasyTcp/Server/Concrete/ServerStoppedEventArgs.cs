using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ServerStoppedEventHandler(object sender, ServerStoppedEventArgs e);

    public class ServerStoppedEventArgs : EventArgs
    {
        public IPEndPoint ListeningPoint { get; internal set; }
    }
}
