using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService
{
    class Program
    {
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> _clienSockets=new List<Socket>();
        private static byte[] _buffer = new byte[1024];
        private static DateTime? _requestTime;
        private static int _warnCount = 0;

        static void Main(string[] args)
        {
            Console.Title = "SERVER";
            StartServer();
            Console.ReadKey();
        }


        private static void StartServer()
        {
            Console.WriteLine("Starting server");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            var socket = _serverSocket.EndAccept(ar);
            _clienSockets.Add(socket);
            Console.WriteLine("Client connected");
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            var received = socket.EndReceive(ar);
            var dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);
            var text = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Text received: "+text);
            var response = text;

            if(_requestTime==null)
            {
                _requestTime = DateTime.Now;
            }
            else
            {
                var ss = DateTime.Now.Subtract((DateTime) _requestTime).TotalSeconds;
                Debug.WriteLine(ss);
                _requestTime=DateTime.Now;
                if (ss <= 1)
                {
                    response += " ---- WARNING! ----";
                    _warnCount++;
                    
                }
            }

            if (_warnCount > 1)
            {
                var closeMessage = Encoding.ASCII.GetBytes("CONNECTION CLOSED!");
                socket.BeginSend(closeMessage, 0, closeMessage.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.Close();
            }
            else
            {
                var data = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            }
            
        }

        private static void SendCallback(IAsyncResult AR)
        {
            var socket = (Socket) AR.AsyncState;
            socket.EndSend(AR);
        }
        
    }
}
