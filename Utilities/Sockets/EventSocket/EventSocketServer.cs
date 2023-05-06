using Utilities.Sockets.SocketFileTransfer;
using Utilities.Sockets.EventSocket.Messages;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Utilities.Sockets.EventSocket
{
    public delegate void HandleMessageEvent(Message message);

    public class EventSocketServer
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TcpListener listener;

        public event HandleMessageEvent HandleMessage;

        private bool listen;

        public EventSocketServer(int port = 30503)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listen = false;
        }

        public void Start()
        {
            logger.Info($">< Start()");
            Task.Run(async () =>
            {
                await StartAsync();
            });
        }

        private async Task StartAsync()
        {
            logger.Info($"> StartAsync()");
            listen = true;
            try
            {
                listener.Start();
                logger.Info("EventSocketServer started");

                while (listen)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    logger.Info("Client connected");

                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
            }
            finally
            {
                listener.Stop();
                logger.Info("EventSocketServer stopped");
            }

            logger.Info($"< StartAsync()");
        }

        public void Stop()
        {
            logger.Info($"> Stop()");
            listen = false;
            logger.Info($"< Stop()");
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            logger.Info($"> HandleClientAsync({client})");
            try
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        logger.Info("Client disconnected");
                        break;
                    }

                    // Deserialize the message object
                    Message message = DeserializeMessage(buffer);

                    // Handle the message
                    HandleMessage?.Invoke(message);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
            }
            finally
            {
                client.Close();
                logger.Info("Client connection closed");
            }
            logger.Info($"< HandleClientAsync({client})");
        }

        /*private void HandleMessage(Message message)
        {
            // Handle the specific message type
            if (message is ItemMovedMessage itemMovedMessage)
            {
                Console.WriteLine($"Item moved to cell {itemMovedMessage.X}/{itemMovedMessage.Y} in backpack {itemMovedMessage.BackpackId} from/to backpack {itemMovedMessage.OptionalFromToBackpackId}");
            }
            else if (message is ItemAddedMessage itemAddedMessage)
            {
                Console.WriteLine($"Item added to cell {itemAddedMessage.X}/{itemAddedMessage.Y} in backpack {itemAddedMessage.BackpackId}");
            }
            else
            {
                throw new ArgumentException("Invalid message type");
            }
        }*/

        private Message DeserializeMessage(byte[] buffer)
        {
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Message)formatter.Deserialize(memoryStream);
            }
        }
    }
}
