using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ServerStartedEventHandler(object sender, ServerStartedEventArgs e);

    public class ServerStartedEventArgs : EventArgs
    {
        public IPEndPoint ListeningPoint { get; internal set; }
    }
}
