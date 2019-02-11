using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyPeasyTcp.Server
{
    public class TcpServerConfig : ITcpServerConfig
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

        public TcpServerConfig(string file)
        {
            iniFile = new FileInfo(file).FullName;
            if (!File.Exists(iniFile))
                CreateDefaultConfiguration();

            ReadIPAddress();
            ReadPort();
            ReadCleanUpPeriod();
            ReadNoMessageTimeout();
            ReadSendBufferSize();
            ReadReceiveBufferSize();
            ReadMaxMessageLength();
        }

        public TcpServerConfig(IPAddress ipAddress, int port)
        {
            IPAddress = ipAddress;
            Port = port;
        }

        #endregion

        #region Properties

        public IPAddress IPAddress { get; private set; } = IPAddress.Any;
        public int Port { get; private set; } = 8080;
        public int CleanUpPeriod { get; private set; } = 10000;
        public int NoMessageTimeout { get; private set; } = 60000;
        public int SendBufferSize { get; private set; } = 1500;
        public int ReceiveBufferSize { get; private set; } = 1500;
        public int MaxMessageLength { get; private set; } = 65536;

        #endregion

        #region Methods

        private void ReadIPAddress()
        {
            IPAddress _IPAddress;
            string temp = GetKey("IPAddress", "Configuration");
            if (temp.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                _IPAddress = IPAddress.Any;
            else if (IPAddress.TryParse(temp, out _IPAddress) == false)
                throw new Exception("Parameter parse error: IPAddress");
            IPAddress = _IPAddress;
        }

        private void ReadPort()
        {
            string temp = GetKey("Port", "Configuration");
            if (int.TryParse(temp, out int _Port) == false)
                throw new Exception("Parameter parse error: Port");
            if ((_Port > 0 && _Port < 65536) == false)
                throw new Exception("Parameter error: Port should be greater than 0 and less then 65536");
            Port = _Port;
        }

        private void ReadCleanUpPeriod()
        {
            string temp = GetKey("CleanUpPeriod", "Configuration");
            if (int.TryParse(temp, out int _CleanUpPeriod) == false)
                throw new Exception("Parameter parse error: CleanUpPeriod");
            if ((_CleanUpPeriod > 0) == false)
                throw new Exception("Parameter error: CleanUpPeriod should be greater than 0");
            CleanUpPeriod = _CleanUpPeriod;
        }

        private void ReadNoMessageTimeout()
        {
            string temp = GetKey("NoMessageTimeout", "Configuration");
            if (int.TryParse(temp, out int _NoMessageTimeout) == false)
                throw new Exception("Parameter parse error: NoMessageTimeout");
            if ((_NoMessageTimeout > 0) == false)
                throw new Exception("Parameter error: NoMessageTimeout should be greater than 0");
            NoMessageTimeout = _NoMessageTimeout;
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
        
            SetKey("IPAddress", "Default", "Configuration");
            SetKey("Port", Port.ToString(), "Configuration");
            SetKey("CleanUpPeriod", CleanUpPeriod.ToString(), "Configuration");
            SetKey("NoMessageTimeout", NoMessageTimeout.ToString(), "Configuration");
            SetKey("SendBufferSize", SendBufferSize.ToString(), "Configuration");
            SetKey("ReceiveBufferSize", ReceiveBufferSize.ToString(), "Configuration");
            SetKey("MaxMessageLength", MaxMessageLength.ToString(), "Configuration");
        }

        #endregion
    }
}
