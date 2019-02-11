using System;
using System.Text;
using EasyPeasyTcp.Server;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var myServer = new TcpServer())
            {
                myServer.ServerStarted += MyServer_ServerStarted;
                myServer.ServerStopped += MyServer_ServerStopped;
                myServer.ServerErrorOccured += MyServer_ServerErrorOccured;
                myServer.ClientConnected += MyServer_ClientConnected;
                myServer.ClientDisconnected += MyServer_ClientDisconnected;
                myServer.ClientMessageReceived += MyServer_ClientMessageReceived;
                myServer.ClientMessageSent += MyServer_ClientMessageSent;
                myServer.ClientErrorOccured += MyServer_ClientErrorOccured;

                Console.WriteLine("--- Easy Peasy TCP Server ---");
                Console.WriteLine("This is a loopback test server that will send back exactly what you have sent");
                Console.WriteLine("Press <ctrl> + z to exit at any time");
                Console.WriteLine();

                myServer.Start(new TcpServerConfig("tcpserver.ini"));
                if (myServer.IsRunning)
                {
                    while (true)
                    {
                        ConsoleKeyInfo info = Console.ReadKey(true);
                        if (info.Key == ConsoleKey.Z && info.Modifiers == ConsoleModifiers.Control)
                            break;
                    }
                }
            }
        }

        private static void MyServer_ClientErrorOccured(object sender, ClientErrorOccuredEventArgs e)
        {
            Print(">> Tcp server encountered an error in client handler", ConsoleColor.Red);
            Print(string.Format("Remote endpoint: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkRed);
            Print(string.Format("Error: {0}", e.ErrorMessage), ConsoleColor.DarkRed);
        }

        private static void MyServer_ClientMessageSent(object sender, ClientMessageSentEventArgs e)
        {
            Print(">> Message sent", ConsoleColor.Gray);
            Print(string.Format("Receiver: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkGray);
            Print(string.Format("Message: \"{0}\"", Encoding.UTF8.GetString(e.SentBytes)), ConsoleColor.DarkGray);
        }

        private static void MyServer_ClientMessageReceived(object sender, ClientMessageReceivedEventArgs e)
        {
            Print(">> New message received", ConsoleColor.Gray);
            Print(string.Format("Sender: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkGray);
            Print(string.Format("Message: \"{0}\"", Encoding.UTF8.GetString(e.ReceivedBytes)), ConsoleColor.DarkGray);

            TcpServer myServer = sender as TcpServer;
            myServer.Send(e.RemoteEndPoint, e.ReceivedBytes);
        }

        private static void MyServer_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Print(">> Client disconnected", ConsoleColor.Red);
            Print(string.Format("Remote endpoint: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkRed);
            Print(string.Format("Local endpoint: {0}/{1}", e.LocalEndPoint.Address, e.LocalEndPoint.Port), ConsoleColor.DarkRed);
        }

        private static void MyServer_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Print(">> New client connected", ConsoleColor.Green);
            Print(string.Format("Remote endpoint: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkGreen);
            Print(string.Format("Local endpoint: {0}/{1}", e.LocalEndPoint.Address, e.LocalEndPoint.Port), ConsoleColor.DarkGreen);
        }

        private static void MyServer_ServerErrorOccured(object sender, ServerErrorOccuredEventArgs e)
        {
            Print(">> Tcp server encountered an error", ConsoleColor.Red);
            Print(string.Format("Error: {0}", e.ErrorMessage), ConsoleColor.DarkRed);
        }

        private static void MyServer_ServerStopped(object sender, ServerStoppedEventArgs e)
        {
            Print(">> Tcp server stopped", ConsoleColor.Gray);
        }

        private static void MyServer_ServerStarted(object sender, ServerStartedEventArgs e)
        {
            Print(">> Tcp server started", ConsoleColor.Gray);
            Print(string.Format("Listening port {0} at address {1}", e.ListeningPoint.Port, e.ListeningPoint.Address), ConsoleColor.DarkGray);
        }

        private static void Print(string message, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}