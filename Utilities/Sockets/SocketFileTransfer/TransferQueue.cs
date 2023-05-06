using Utilities.Sockets.SocketFileTransfer.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Sockets.SocketFileTransfer
{

    public enum QueueType : byte
    {
        Download,
        Upload
    }

    public class TransferQueue
    {
        public static Random Random = new Random();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static TransferQueue CreateUploadQueue(TransferClient client, string fileName)
        {
            logger.Info($"> CreateUploadQueue(client: {client}, fileName: {fileName})");
            try
            {
                var queue = new TransferQueue();
                queue.Filename = fileName;
                queue.Client = client;
                queue.Type = QueueType.Upload;
                queue.FS = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                queue.Thread = new Thread(new ParameterizedThreadStart(transferProc));
                queue.Thread.IsBackground = true;
                queue.Id = Random.Next();
                queue.Length = queue.FS.Length;
                queue.SelfCreated = true;

                logger.Info($"< CreateUploadQueue(client: {client}, fileName: {fileName}).return(queue: [{queue}])");
                return queue;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Whoeps, something went wrong");
                logger.Info($"< CreateUploadQueue(client: {client}, fileName: {fileName}).return(null)");
                return null;
            }
        }
        public static TransferQueue CreateDownloadQueue(TransferClient client, int id, string saveName, long length)
        {
            logger.Info($"> CreateDownloadQueue(client: {client}, id: {id}, saveName: {saveName}, length: {length})");
            try
            {
                var queue = new TransferQueue();
                queue.Filename = Path.GetFileName(saveName);
                queue.Client = client;
                queue.Type = QueueType.Download;
                queue.FS = new FileStream(saveName, FileMode.Create);
                queue.FS.SetLength(length);
                queue.Length = length;
                queue.Id = id;
                queue.SelfCreated = false;
                logger.Info($"< CreateDownloadQueue(client: {client}, id: {id}, saveName: {saveName}, length: {length}).return(queue: [{queue}])");
                return queue;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Whoeps, something went wrong");
                logger.Info($"< CreateDownloadQueue(client: {client}, id: {id}, saveName: {saveName}, length: {length}).return(null)");
                return null;
            }
        }


        private const int FILE_BUFFER_SIZE = 8175;
        private static byte[] file_buffer = new byte[FILE_BUFFER_SIZE];

        private ManualResetEvent pauseEvent;

        public int Id { get; set; }
        public int Progress { get; set; }
        public int LastProgress { get; set; }

        public long Transferred { get; set; }
        public long Index { get; set; }
        public long Length { get; set; }

        public bool Running { get; set; }
        public bool Paused { get; set; }
        public bool SelfCreated { get; set; }

        public string Filename { get; set; }

        public QueueType Type { get; set; }

        public TransferClient Client { get; set; }
        public Thread Thread { get; set; }
        public FileStream FS { get; set; }

        private TransferQueue()
        {
            logger.Info("> TransferQueue()");
            pauseEvent = new ManualResetEvent(true);
            Running = true;
            logger.Info("< TransferQueue()");
        }

        public void Start()
        {
            logger.Info("> Start()");
            Running = true;
            Thread.Start(this);
            logger.Info("< Start()");
        }

        public void Stop()
        {
            logger.Info("> Stop()");
            Running = false;
            logger.Info("< Stop()");
        }

        public void Pause()
        {
            logger.Info("> Pause()");
            if (!Paused)
            {
                pauseEvent.Reset();
            }
            else
            {
                pauseEvent.Set();
            }

            Paused = !Paused;
            logger.Info("< Pause()");
        }

        public void Close()
        {
            logger.Info("> Close()");
            try
            {
                Client.Transfers.Remove(Id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Whoeps, something went wrong");
            }

            Running = false;
            FS.Close();
            pauseEvent.Dispose();

            Client = null;
            logger.Info("< Close()");
        }

        public void Write(byte[] bytes, long index)
        {
            logger.Trace("> Write()");
            lock (this)
            {
                FS.Position = index;
                FS.Write(bytes, 0, bytes.Length);
                Transferred += bytes.Length;
            }
            logger.Trace("< Write()");
        }

        private static void transferProc(object obj)
        {
            TransferQueue queue = (TransferQueue)obj;
            logger.Debug($"> transferProc(queue: {queue})");

            while (queue.Running && queue.Index < queue.Length)
            {
                queue.pauseEvent.WaitOne();

                if (!queue.Running)
                {
                    break;
                }

                lock(file_buffer)
                {
                    queue.FS.Position = queue.Index;

                    int read = queue.FS.Read(file_buffer, 0, file_buffer.Length);
                    PacketWriter packetWriter = new PacketWriter();
                    packetWriter.Write((byte)Headers.Chunk);
                    packetWriter.Write(queue.Id);
                    packetWriter.Write(queue.Index);
                    packetWriter.Write(read);
                    packetWriter.Write(file_buffer, 0, read);

                    queue.Transferred += read;
                    queue.Index += read;

                    queue.Client.Send(packetWriter.GetBytes());
                    queue.Progress = (int)((queue.Transferred * 100) / queue.Length);

                    if (queue.LastProgress < queue.Progress)
                    {
                        queue.LastProgress = queue.Progress;
                        queue.Client.callProgressChanged(queue);
                    }

                    Thread.Sleep(1);
                }
            }

            queue.Client?.callCompleted(queue);
            logger.Debug($"< transferProc(queue: {queue})");
            queue.Close();
        }

        public override string ToString()
        {
            return $"client: {Client.ToString()}; id: {Id}; type: {Type}; filename:{Filename}";
        }
    }
}
