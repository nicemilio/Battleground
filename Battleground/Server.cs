using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Battleground
{
    internal class Server
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        private bool consoleWrite;

        public Server(bool consoleWrite = true)
        {
            int count = 0;
            this.consoleWrite = consoleWrite;
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();
            if (this.consoleWrite) Console.WriteLine("Server started!");
            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);
                if (this.consoleWrite) Console.WriteLine("Someone connected!!");

                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
                if (list_clients.Count() == 2) startgame();
            }
        }

        public void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data, id);
                if (this.consoleWrite) Console.WriteLine(data);
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private void startgame()
        {
            //TODO add something to randomise the starting player
            if (this.consoleWrite) Console.WriteLine("starting the game");
            broadcast("first!", 0);
            broadcast("second!", 1);
        }

        public void broadcast(string data, int clientID)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (var c in list_clients)
                {
                    if (c.Key == clientID) continue; //Don't send the message to the sender
                    NetworkStream stream = c.Value.GetStream();
                    if (this.consoleWrite) Console.WriteLine(data + " , send to client number: " + c.Key);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
