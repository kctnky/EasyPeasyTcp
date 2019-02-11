# EasyPeasyTcp

It is a byte oriented synchronous wrapper implementation for advanced communication needs through TCP sockets. Multiple clients are supported. 

Messages are being transmitted in frames between peers, starting with STX and ending with EOT characters. Byte stuffing is used inside the message content for escaping those characters. Code in the library handles all the work related to constructing/extracting frames. So, consumers of this library are able to send and receive one message at a time regardless of its size without any headache. 

Server implementation includes a resource cleaner such that unused open connections are forced to be closed after a predefined no-message-timeout. So, possible client applications referencing this library are supposed to send handshake messages to keep the connection alive. 

Configuration settings for both server and client implementatios are extracted to ini files to be placed in the same directory of executing assembly. 

Basic implementation of loopback server is being followed. Note that configuration settings like Ip address or port number to be listened are read from tcpserver.ini whose content is also given below.

```
static void Main(string[] args)
{
    using (var myServer = new TcpServer())
    {
        myServer.ClientConnected += MyServer_ClientConnected;
        myServer.ClientDisconnected += MyServer_ClientDisconnected;
        myServer.ClientMessageReceived += MyServer_ClientMessageReceived;
        myServer.Start(new TcpServerConfig("tcpserver.ini"));
        Console.ReadKey();
    }
}

static void MyServer_ClientMessageReceived(object sender, ClientMessageReceivedEventArgs e)
{
    TcpServer myServer = sender as TcpServer;
    myServer.Send(e.RemoteEndPoint, e.ReceivedBytes);
}

static void MyServer_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
{
    // do whatever..
}

static void MyServer_ClientConnected(object sender, ClientConnectedEventArgs e)
{
    // do whatever..
}
```

tcpserver.ini
```
[Configuration]
IPAddress=Default
Port=8080
CleanUpPeriod=10000
NoMessageTimeout=20000
SendBufferSize=1500
ReceiveBufferSize=1500
MaxMessageLength=65536
```

Basic implementation of client application is being followed. Note that configuration settings like Ip address or port number to be connected are read from tcpclient.ini whose content is also given below.

```
static void Main(string[] args)
{
    using (var myClient = new TcpClient())
    {
        myClient.MessageReceived += MyClient_MessageReceived;
        myClient.Connect(new TcpClientConfig("tcpclient.ini"));
        while (true)
        {
            string line = Console.ReadLine();
            myClient.Send(Encoding.UTF8.GetBytes(line));
        }
    }
}

static void MyClient_MessageReceived(object sender, MessageReceivedEventArgs e)
{
    string received = string.Format("Message received: \"{0}\"", Encoding.UTF8.GetString(e.ReceivedBytes));
    Console.WriteLine(received);
}
```

tcpclient.ini
```
[Configuration]
ServerIPAddress=127.0.0.1
ServerPort=8080
SendBufferSize=1500
ReceiveBufferSize=1500
MaxMessageLength=65536
```

Please check TestServer and TestClient implementations for more details about how to use this library.
