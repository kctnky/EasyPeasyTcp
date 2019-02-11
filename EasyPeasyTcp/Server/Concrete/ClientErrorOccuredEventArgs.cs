using System;
using System.Net;

namespace EasyPeasyTcp.Server
{
    public delegate void ClientErrorOccuredEventHandler(object sender, ClientErrorOccuredEventArgs e);

    public class ClientErrorOccuredEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
        public string ErrorMessage { get; internal set; }
    }
}
