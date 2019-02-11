using System.Net;

namespace EasyPeasyTcp.Server
{
    public interface ITcpServerConfig
    {
        IPAddress IPAddress { get; }
        int Port { get; }
        int CleanUpPeriod { get; }
        int NoMessageTimeout { get; }
        int SendBufferSize { get; }
        int ReceiveBufferSize { get; }
        int MaxMessageLength { get; }
    }
}
