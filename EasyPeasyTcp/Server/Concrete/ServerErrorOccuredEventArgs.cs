using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ServerErrorOccuredEventHandler(object sender, ServerErrorOccuredEventArgs e);

    public class ServerErrorOccuredEventArgs : EventArgs
    {
        public IPEndPoint ListeningPoint { get; internal set; }
        public string ErrorMessage { get; internal set; }
    }
}
