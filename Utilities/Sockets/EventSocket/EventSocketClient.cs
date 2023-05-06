using Utilities.Sockets.EventSocket.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Sockets.EventSocket
{
    public class EventSocketClient
    {
        private readonly TcpClient client;

        public EventSocketClient(string serverIp, int port = 30503)
        {
            client = new TcpClient(serverIp, port);
        }

        public async Task SendMessageAsync(Message message)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                byte[] buffer = SerializeMessage(message);

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private byte[] SerializeMessage(Message message)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, message);
                return memoryStream.ToArray();
            }
        }
    }
}
