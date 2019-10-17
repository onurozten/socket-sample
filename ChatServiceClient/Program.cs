using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServiceClient
{
    class Program
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "CLÝENT";
            LoopConnect();
            SendLoop();
            //Console.ReadLine();
        }

        private static void SendLoop()
        {
            while (true)
            {
                Console.Write("Enter message: ");
                var req = Console.ReadLine();
                var buffer = Encoding.ASCII.GetBytes(req);
                _clientSocket.Send(buffer);

                var receivedBuf = new byte[1024];
                var rec = _clientSocket.Receive(receivedBuf);

                var data = new byte[rec];
                Array.Copy(receivedBuf, data, rec);
                var receivedText = Encoding.ASCII.GetString(data);
                Console.WriteLine("Received: "+receivedText);

                if (receivedText == "CONNECTION CLOSED!")
                {
                    break;
                }
            }
            
        }

        private static void LoopConnect()
        {
            var attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, 100);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Connection attempts: "+ attempts);
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");

        }

    }
}
