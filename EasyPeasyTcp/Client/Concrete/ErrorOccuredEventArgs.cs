using System;
using System.Net;

namespace EasyPeasyTcp.Client
{
    public delegate void ErrorOccuredEventHandler(object sender, ErrorOccuredEventArgs e);

    public class ErrorOccuredEventArgs : EventArgs
    {
        public IPEndPoint LocalEndPoint { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }
        public string ErrorMessage { get; internal set; }
    }
}
