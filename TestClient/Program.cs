using System;
using System.Text;
using EasyPeasyTcp.Client;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var myClient = new TcpClient())
            {
                myClient.Connected += MyClient_Connected;
                myClient.Disconnected += MyClient_Disconnected;
                myClient.MessageReceived += MyClient_MessageReceived;
                myClient.MessageSent += MyClient_MessageSent;
                myClient.ErrorOccured += MyClient_ErrorOccured;

                Console.WriteLine("--- Easy Peasy TCP Client ---");
                Console.WriteLine("Type something and press <enter> to send");
                Console.WriteLine("Type \"quit\" to exit");
                Console.WriteLine();

                myClient.Connect(new TcpClientConfig("tcpclient.ini"));
                while (true)
                {
                    string line = Console.ReadLine();
                    if (line != null && line.Equals("quit", StringComparison.InvariantCultureIgnoreCase))
                        break;
                    else
                        myClient.Send(Encoding.UTF8.GetBytes(line));
                }
            }
        }

        private static void MyClient_ErrorOccured(object sender, ErrorOccuredEventArgs e)
        {
            Print(">> Tcp client encountered an error", ConsoleColor.Red);
            Print(string.Format("Error: {0}", e.ErrorMessage), ConsoleColor.DarkRed);
        }

        private static void MyClient_MessageSent(object sender, MessageSentEventArgs e)
        {
            Print(">> Message sent", ConsoleColor.Gray);
            Print(string.Format("Message: \"{0}\"", Encoding.UTF8.GetString(e.SentBytes)), ConsoleColor.DarkGray);
        }

        private static void MyClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Print(">> New message received", ConsoleColor.Gray);
            Print(string.Format("Message: \"{0}\"", Encoding.UTF8.GetString(e.ReceivedBytes)), ConsoleColor.DarkGray);
        }

        private static void MyClient_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Print(">> Disconnected from server", ConsoleColor.Red);
            Print(string.Format("Remote endpoint: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkRed);
            Print(string.Format("Local endpoint: {0}/{1}", e.LocalEndPoint.Address, e.LocalEndPoint.Port), ConsoleColor.DarkRed);
        }

        private static void MyClient_Connected(object sender, ConnectedEventArgs e)
        {
            Print(">> Connected to server", ConsoleColor.Green);
            Print(string.Format("Remote endpoint: {0}/{1}", e.RemoteEndPoint.Address, e.RemoteEndPoint.Port), ConsoleColor.DarkGreen);
            Print(string.Format("Local endpoint: {0}/{1}", e.LocalEndPoint.Address, e.LocalEndPoint.Port), ConsoleColor.DarkGreen);
        }

        private static void Print(string message, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}