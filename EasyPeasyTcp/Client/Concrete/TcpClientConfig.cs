using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyPeasyTcp.Client
{
    public class TcpClientConfig : ITcpClientConfig
    {
        #region External References

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        #endregion

        #region Constant Fields

        private const int MAX_PARAM_LENGTH = 255;

        #endregion

        #region Fields

        private readonly string iniFile;

        #endregion

        #region Constructers

        public TcpClientConfig(string file)
        {
            iniFile = new FileInfo(file).FullName;
            if (!File.Exists(iniFile))
                CreateDefaultConfiguration();

            ReadServerIPAddress();
            ReadServerPort();
            ReadSendBufferSize();
            ReadReceiveBufferSize();
            ReadMaxMessageLength();
        }

        public TcpClientConfig(IPAddress serverIPAddress, int serverPort)
        {
            ServerIPAddress = serverIPAddress;
            ServerPort = serverPort;
        }

        #endregion

        #region Properties

        public IPAddress ServerIPAddress { get; private set; } = IPAddress.Parse("127.0.0.1");
        public int ServerPort { get; private set; } = 8080;
        public int SendBufferSize { get; private set; } = 1500;
        public int ReceiveBufferSize { get; private set; } = 1500;
        public int MaxMessageLength { get; private set; } = 65536;

        #endregion

        #region Methods

        private void ReadServerIPAddress()
        {
            IPAddress _IPAddress;
            string temp = GetKey("ServerIPAddress", "Configuration");
            if (IPAddress.TryParse(temp, out _IPAddress) == false)
                throw new Exception("Parameter parse error: ServerIPAddress");
            ServerIPAddress = _IPAddress;
        }

        private void ReadServerPort()
        {
            string temp = GetKey("ServerPort", "Configuration");
            if (int.TryParse(temp, out int _Port) == false)
                throw new Exception("Parameter parse error: ServerPort");
            if ((_Port > 0 && _Port < 65536) == false)
                throw new Exception("Parameter error: ServerPort should be greater than 0 and less then 65536");
            ServerPort = _Port;
        }

        private void ReadSendBufferSize()
        {
            string temp = GetKey("SendBufferSize", "Configuration");
            if (int.TryParse(temp, out int _SendBufferSize) == false)
                throw new Exception("Parameter parse error: SendBufferSize");
            if ((_SendBufferSize > 0) == false)
                throw new Exception("Parameter error: SendBufferSize should be greater than 0");
            SendBufferSize = _SendBufferSize;
        }

        private void ReadReceiveBufferSize()
        {
            string temp = GetKey("ReceiveBufferSize", "Configuration");
            if (int.TryParse(temp, out int _ReceiveBufferSize) == false)
                throw new Exception("Parameter parse error: ReceiveBufferSize");
            if ((_ReceiveBufferSize > 0) == false)
                throw new Exception("Parameter error: ReceiveBufferSize should be greater than 0");
            ReceiveBufferSize = _ReceiveBufferSize;
        }

        private void ReadMaxMessageLength()
        {
            string temp = GetKey("MaxMessageLength", "Configuration");
            if (int.TryParse(temp, out int _MaxMessageLength) == false)
                throw new Exception("Parameter parse error: MaxMessageLength");
            if ((_MaxMessageLength > 0) == false)
                throw new Exception("Parameter error: MaxMessageLength should be greater than 0");
            MaxMessageLength = _MaxMessageLength;
        }

        private string GetKey(string key, string section)
        {
            StringBuilder value = new StringBuilder(MAX_PARAM_LENGTH);
            GetPrivateProfileString(section, key, string.Empty, value, MAX_PARAM_LENGTH, iniFile);
            return value.ToString();
        }

        private void SetKey(string key, string value, string section)
        {
            WritePrivateProfileString(section, key, value, iniFile);
        }

        private void CreateDefaultConfiguration()
        {
            using (var f = File.Create(iniFile))
                f.Close();
        
            SetKey("ServerIPAddress", ServerIPAddress.ToString(), "Configuration");
            SetKey("ServerPort", ServerPort.ToString(), "Configuration");
            SetKey("SendBufferSize", SendBufferSize.ToString(), "Configuration");
            SetKey("ReceiveBufferSize", ReceiveBufferSize.ToString(), "Configuration");
            SetKey("MaxMessageLength", MaxMessageLength.ToString(), "Configuration");
        }

        #endregion
    }
}
