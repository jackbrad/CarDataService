using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NascarTelnetServer
{
    public enum EClientState
    {
        NotLogged = 0,
        Logging = 1,
        LoggedIn = 2
    }

    public class Client
    {
        public IPEndPoint RemoteEndPoint;
        public DateTime ConnectedAt;
        public EClientState ClientState;
        public string CommandIssued = string.Empty;

        public Client(IPEndPoint remoteEndPoint, DateTime connectedAt, EClientState clientState)
        {
            RemoteEndPoint = remoteEndPoint;
            ConnectedAt = connectedAt;
            ClientState = clientState;
        }
    }

    class Program
    {
        private static Socket _serverSocket;
        private static readonly byte[] Data = new byte[DataSize];
        private static bool _newClients = true;
        private const int DataSize = 1024;
        private static readonly Dictionary<Socket, Client> ClientList = new Dictionary<Socket, Client>();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            new Thread(BackgroundThread) { IsBackground = false }.Start();
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(IPAddress.Any, int.Parse(ConfigurationManager.AppSettings["Port"]));
            _serverSocket.Bind(endPoint);
            _serverSocket.Listen(0);
            _serverSocket.BeginAccept(AcceptConnection, _serverSocket);
            Console.WriteLine("Server socket listening to upcoming connections.");
        }

        private static void BackgroundThread()
        {
            while (true)
            {
                var input = Console.ReadLine();

                if (input == "clients")
                {
                    if (ClientList.Count == 0) continue;
                    var clientNumber = 0;
                    foreach (var currentClient in ClientList.Select(client => client.Value))
                    {
                        clientNumber++;
                        Console.WriteLine("Client #{0} (From: {1}:{2}, ECurrentState: {3}, Connection time: {4})", clientNumber, currentClient.RemoteEndPoint.Address, currentClient.RemoteEndPoint.Port, currentClient.ClientState, currentClient.ConnectedAt);
                    }
                }

                if (input != null && input.StartsWith("kill"))
                {
                    var _Input = input.Split(' ');
                    int clientID = 0;
                    try
                    {
                        if (Int32.TryParse(_Input[1], out clientID) && clientID >= ClientList.Keys.Count)
                        {
                            int currentClient = 0;
                            foreach (Socket currentSocket in ClientList.Keys.ToArray())
                            {
                                currentClient++;
                                if (currentClient == clientID)
                                {
                                    currentSocket.Shutdown(SocketShutdown.Both);
                                    currentSocket.Close();
                                    ClientList.Remove(currentSocket);
                                    Console.WriteLine("Client has been disconnected and cleared up.");
                                }
                            }
                        }
                        else { Console.WriteLine("Could not kick client: invalid client number specified."); }
                    }
                    catch { Console.WriteLine("Could not kick client: invalid client number specified."); }
                }

                if (input == "killall")
                {
                    int deletedClients = 0;
                    foreach (Socket currentSocket in ClientList.Keys.ToArray())
                    {
                        currentSocket.Shutdown(SocketShutdown.Both);
                        currentSocket.Close();
                        ClientList.Remove(currentSocket);
                        deletedClients++;
                    }

                    Console.WriteLine("{0} clients have been disconnected and cleared up.", deletedClients);
                }

                if (input == "lock") { _newClients = false; Console.WriteLine("Refusing new connections."); }
                if (input == "unlock") { _newClients = true; Console.WriteLine("Accepting new connections."); }
            }
        }

        private static void AcceptConnection(IAsyncResult result)
        {
            if (!_newClients) return;
            var oldSocket = (Socket)result.AsyncState;
            var newSocket = oldSocket.EndAccept(result);
            var client = new Client((IPEndPoint)newSocket.RemoteEndPoint, DateTime.Now, EClientState.NotLogged);
            ClientList.Add(newSocket, client);
            Console.WriteLine("Client connected. (From: " + $"{client.RemoteEndPoint.Address}:{client.RemoteEndPoint.Port}" + ")");
            //var message = System.IO.File.ReadAllBytes(ConfigurationManager.AppSettings["SourceFilePath"]);
            var lines = System.IO.File.ReadAllLines(ConfigurationManager.AppSettings["SourceFilePath"]);
            client.ClientState = EClientState.Logging;
            //var message = Encoding.ASCII.GetBytes(output);
            var stuff = lines.Where(x => x.StartsWith("$NMGT;88;")).Select(l => Encoding.UTF8.GetBytes(l));
            try
            {
                foreach (var data in stuff)
                {
                    //Thread.Sleep(200);
                    newSocket.BeginSend(data, 0, data.Length, SocketFlags.None, ar =>
                    {
                        var clientSocket = (Socket)ar.AsyncState;
                        clientSocket.EndSend(ar);
                    }, newSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private static void ReceiveData(IAsyncResult result)
        {
            try
            {
                var clientSocket = (Socket)result.AsyncState;
                Client client;
                ClientList.TryGetValue(clientSocket, out client);
                var received = clientSocket.EndReceive(result);
                if (received == 0)
                {
                    clientSocket.Close();
                    ClientList.Remove(clientSocket);
                    _serverSocket.BeginAccept(AcceptConnection, _serverSocket);
                    Console.WriteLine(
                        $"Client disconnected. (From: {client.RemoteEndPoint.Address}:{client.RemoteEndPoint.Port}" +
                        ")");
                    return;
                }

                Console.WriteLine("Received '{0}' (From: {1}:{2})", BitConverter.ToString(Data, 0, received), client.RemoteEndPoint.Address, client.RemoteEndPoint.Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}