using System.Net.Sockets;

namespace SimpleTcpChat
{
    internal class ClientManager
    {
        private Dictionary<String, Socket> clientDict { get; set; } = [];
        public Dictionary<String, Socket> ClientDict { get { return clientDict; } }
        
        public bool AddNewClient(String clientName, Socket socket)
        {
            clientDict.Add(clientName, socket);
            return true;
        }
        public bool RemoveClient(String clientName)
        {
            clientDict.Remove(clientName);
            return true;
        }
    }
}
