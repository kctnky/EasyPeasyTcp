using System.Net;

namespace EasyPeasyTcp.Client
{
    public interface ITcpClientConfig
    {
        IPAddress ServerIPAddress { get; }
        int ServerPort { get; }
        int SendBufferSize { get; }
        int ReceiveBufferSize { get; }
        int MaxMessageLength { get; }
    }
}
