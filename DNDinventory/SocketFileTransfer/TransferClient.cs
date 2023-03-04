using DNDinventory.Model;
using DNDinventory.SocketFileTransfer.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.SocketFileTransfer
{
    public delegate void TransferEventHandler(object sender, TransferQueue queue);
    public delegate void ConnectCallback(object sender, string error);

    public class TransferClient
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Socket _baseSocket;
        private byte[] _buffer = new byte[8192];
        private ConnectCallback _connectCallback;
        private Dictionary<int, TransferQueue> _transfers;
        public Dictionary<int, TransferQueue> Transfers
        {
            get { return _transfers; }
        }

        public bool Closed { get; private set; }
        public string OutputFolder { get; set; }
        public IPEndPoint EndPoint { get; private set; }
        public DmTabItem DmTabItem { get; set; }

        public event TransferEventHandler Queued;
        public event TransferEventHandler ProgressChanged;
        public event TransferEventHandler Stopped;
        public event TransferEventHandler Complete;
        public event TransferEventHandler Paused;
        public event EventHandler Disconnected;

        public TransferClient()
        {
            logger.Info("> TransferClient()");
            _baseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _transfers = new Dictionary<int, TransferQueue>();
            logger.Info("< TransferClient()");
        }

        public TransferClient(Socket socket)
        {
            logger.Info($"> TransferClient(socket: {socket})");
            _baseSocket = socket;
            EndPoint = (IPEndPoint)_baseSocket.RemoteEndPoint;
            _transfers = new Dictionary<int, TransferQueue>();
            logger.Info($"< TransferClient(socket: {socket})");
        }

        public void Connect(string hostName, int port, ConnectCallback callback)
        {
            logger.Info($"> Connect(hostName: {hostName}, port: {port}), callback {callback}");
            _connectCallback = callback;
            _baseSocket.BeginConnect(hostName, port, connectCallback, null);
            logger.Info($"< Connect(hostName: {hostName}, port: {port}), callback {callback}");
        }

        private void connectCallback(IAsyncResult asyncResult)
        {
            logger.Info($"> Connect(AsyncResult: {asyncResult})");
            string error = null;
            try
            {
                _baseSocket.EndConnect(asyncResult);
                EndPoint = (IPEndPoint)_baseSocket.RemoteEndPoint;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                logger.Error(ex, "Whoeps, something went wrong");
            }

            _connectCallback(this, error);
            logger.Info($"< Connect(AsyncResult: {asyncResult})");
        }

        public void Run()
        {
            logger.Info("> Run()");
            try
            {
                _baseSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.Peek, receiveCallback, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Whoeps, something went wrong");
            }
            logger.Info("< Run()");
        }

        public void QueueTransfer(string fileName)
        {
            logger.Info("> QueueTransfer()");
            try
            {
                TransferQueue queue = TransferQueue.CreateUploadQueue(this, fileName);
                _transfers.Add(queue.Id, queue);
                PacketWriter packetWriter = new PacketWriter();
                packetWriter.Write((byte)Headers.Queue);
                packetWriter.Write(queue.Id);
                packetWriter.Write(queue.Filename);
                packetWriter.Write(queue.Length);
                Send(packetWriter.GetBytes());

                Queued?.Invoke(this, queue);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Whoeps, something went wrong");
            }
            logger.Info("< QueueTransfer()");
        }

        public void StartTransfer(TransferQueue queue)
        {
            logger.Info($"> StartTransfer(queue: [{queue}])");
            PacketWriter packetWriter = new PacketWriter();
            packetWriter.Write((byte)Headers.Start);
            packetWriter.Write(queue.Id);
            Send(packetWriter.GetBytes());
            logger.Info($"< StartTransfer(queue: [{queue}])");
        }

        public void StopTransfer(TransferQueue queue)
        {
            logger.Info($"> StopTransfer(queue: [{queue}])");
            if (queue.Type == QueueType.Upload)
            {
                queue.Stop();
            }

            Stopped?.Invoke(this, queue);

            PacketWriter packetWriter = new PacketWriter();
            packetWriter.Write((byte)Headers.Stop);
            packetWriter.Write(queue.Id);
            Send(packetWriter.GetBytes());
            queue.Close();
            logger.Info($"< StopTransfer(queue: [{queue}])");
        }

        public void PauseTransfer(TransferQueue queue)
        {
            logger.Info($"> PauseTransfer(queue: [{queue}])");
            if (queue.Type==QueueType.Upload)
            {
                queue.Pause();
            }

            Paused?.Invoke(this, queue);

            PacketWriter packetWriter = new PacketWriter();
            packetWriter.Write((byte)Headers.Pause);
            packetWriter.Write(queue.Id);
            Send(packetWriter.GetBytes());
            logger.Info($"< PauseTransfer(queue: [{queue}])");
        }

        public int GetOverallProgress()
        {
            int overall = 0;

            if (_transfers != null)
            {
                if (_transfers.Count > 0)
                {
                    logger.Trace("> GetOverallProgress()");
                    foreach (var pair in _transfers)
                    {
                        overall += pair.Value.Progress;
                    }

                    if (overall > 0)
                    {
                        overall = (overall) / (_transfers.Count);
                    }
                    logger.Trace($"< GetOverallProgress().return({overall})");
                }
            }

            return overall;
        }

        public void Send(byte[] data)
        {
            logger.Debug($"> Send(data: {data})");
            if (Closed)
            {
                logger.Warn("Client closed");
                logger.Debug($"< Send(data: {data})");
                return;
            }

            lock (this)
            {
                try
                {
                    _baseSocket.Send(BitConverter.GetBytes(data.Length), 0, 4, SocketFlags.None);
                    _baseSocket.Send(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Close();
                    logger.Error(ex, "Whoeps, something went wrong");
                }
            }
            logger.Debug($"< Send(data: {data})");
        }

        public void Close()
        {
            logger.Info("> Close()");
            Closed = true;
            _baseSocket.Close();

            if (_transfers != null)
            {
                _transfers.Clear();
                _transfers = null;
            }
            _buffer = null;
            OutputFolder = null;

            Disconnected?.Invoke(this, EventArgs.Empty);
            logger.Info("< Close()");
        }

        private void process()
        {
            PacketReader packetReader = new PacketReader(_buffer);

            Headers header = (Headers)packetReader.ReadByte();
            logger.Trace($"> process(header: {header})");

            switch (header)
            {
                case Headers.Queue:
                    {
                        int id = packetReader.ReadInt32();
                        string fileName = packetReader.ReadString();
                        long length = packetReader.ReadInt64();
                        logger.Trace($"Queue id: {id}, fileName: {fileName}, length: {length}");

                        TransferQueue queue = TransferQueue.CreateDownloadQueue(this, id, Path.Combine(OutputFolder, Path.GetFileName(fileName)), length);

                        _transfers.Add(id, queue);

                        Queued?.Invoke(this, queue);
                    }
                    break;
                case Headers.Start:
                    {
                        int id = packetReader.ReadInt32();
                        logger.Trace($"Start id: {id}");

                        if (_transfers.ContainsKey(id))
                        {
                            _transfers[id].Start();
                        }
                    }
                    break;
                case Headers.Stop:
                    {
                        int id = packetReader.ReadInt32();
                        logger.Trace($"Stop id: {id}");

                        if (_transfers.ContainsKey(id))
                        {
                            TransferQueue queue = _transfers[id];
                            queue.Stop();
                            queue.Close();

                            Stopped?.Invoke(this, queue);

                            _transfers.Remove(id);
                        }
                    }
                    break;
                case Headers.Pause:
                    {
                        int id = packetReader.ReadInt32();
                        logger.Trace($"Pause id: {id}");

                        if (_transfers.ContainsKey(id))
                        {
                            TransferQueue queue = _transfers[id];

                            if (queue.Type == QueueType.Upload)
                            {
                                queue.Pause();
                            }

                            Paused?.Invoke(this, queue);
                        }
                    }
                    break;
                case Headers.Chunk:
                    {
                        int id = packetReader.ReadInt32();
                        long index = packetReader.ReadInt64();
                        int size = packetReader.ReadInt32();
                        byte[] buffer = packetReader.ReadBytes(size);
                        logger.Trace($"id: {id}, index: {index}, size: {size}, buffer: {buffer}");

                        TransferQueue queue = _transfers[id];

                        queue.Write(buffer, index);
                        queue.Progress = (int)((queue.Transferred * 100) / queue.Length);

                        if (queue.LastProgress<queue.Progress)
                        {
                            queue.LastProgress = queue.Progress;

                            ProgressChanged?.Invoke(this, queue);

                            if (queue.Progress==100)
                            {
                                queue.Close();

                                Complete?.Invoke(this, queue);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            packetReader.Dispose();
            logger.Trace($"< process(header: {header.ToString()})");
        }

        private void receiveCallback(IAsyncResult asyncResult)
        {
            logger.Trace($"> receiveCallback(AsyncResult: {asyncResult})");
            try
            {
                int found = _baseSocket.EndReceive(asyncResult);

                if (found>=4)
                {
                    _baseSocket.Receive(_buffer, 0, 4, SocketFlags.None);

                    int size = BitConverter.ToInt32(_buffer, 0);
                    int read = _baseSocket.Receive(_buffer, 0, size, SocketFlags.None);

                    while(read<size)
                    {
                        read += _baseSocket.Receive(_buffer, read, size - read, SocketFlags.None);
                    }
                    process();
                }

                Run();
            }
            catch (Exception ex)
            {
                Close();
                logger.Error(ex, "Whoeps, something went wrong");
            }
            logger.Trace($"< receiveCallback(AsyncResult: {asyncResult})");
        }

        internal void callProgressChanged(TransferQueue queue)
        {
            logger.Trace($"> callProgressChanged(queue: {queue})");
            ProgressChanged?.Invoke(this, queue);
            logger.Trace($"< callProgressChanged(queue: {queue})");
        }

        internal void callCompleted(TransferQueue queue)
        {
            logger.Trace($"> callCompleted(queue: {queue})");
            Complete?.Invoke(this, queue);
            logger.Trace($"< callCompleted(queue: {queue})");
        }

        public override string ToString()
        {
            return $"{EndPoint.ToString()}";
        }
    }
}
