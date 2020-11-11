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

        public event TransferEventHandler Queued;
        public event TransferEventHandler ProgressChanged;
        public event TransferEventHandler Stopped;
        public event TransferEventHandler Complete;
        public event TransferEventHandler Paused;
        public event EventHandler Disconnected;

        public TransferClient()
        {
            _baseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _transfers = new Dictionary<int, TransferQueue>();
        }

        public TransferClient(Socket socket)
        {
            _baseSocket = socket;
            EndPoint = (IPEndPoint)_baseSocket.RemoteEndPoint;
            _transfers = new Dictionary<int, TransferQueue>();
        }

        public void Connect(string hostName, int port, ConnectCallback callback)
        {
            _connectCallback = callback;
            _baseSocket.BeginConnect(hostName, port, connectCallback, null);
        }

        private void connectCallback(IAsyncResult asyncResult)
        {
            string error = null;
            try
            {
                _baseSocket.EndConnect(asyncResult);
                EndPoint = (IPEndPoint)_baseSocket.RemoteEndPoint;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            _connectCallback(this, error);
        }

        public void Run()
        {
            try
            {
                _baseSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.Peek, receiveCallback, null);
            }
            catch (Exception)
            {

            }
        }

        public void QueueTransfer(string fileName)
        {
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

                if (Queued != null)
                {
                    Queued(this, queue);
                }
            }
            catch (Exception)
            {
            }
        }

        public void StartTransfer(TransferQueue queue)
        {
            PacketWriter packetWriter = new PacketWriter();
            packetWriter.Write((byte)Headers.Start);
            packetWriter.Write(queue.Id);
            Send(packetWriter.GetBytes());
        }

        public void StopTransfer(TransferQueue queue)
        {
            if (queue.Type == QueueType.Upload)
            {
                queue.Stop();
            }

            if (Stopped != null)
            {
                Stopped(this, queue);
            }

            PacketWriter packetWriter = new PacketWriter();
            packetWriter.Write((byte)Headers.Stop);
            packetWriter.Write(queue.Id);
            Send(packetWriter.GetBytes());
            queue.Close();
        }

        public void PauseTransfer(TransferQueue queue)
        {
            if (queue.Type==QueueType.Upload)
            {
                queue.Pause();
            }

            if (Paused != null)
            {
                Paused(this, queue);
            }

            PacketWriter packetWriter = new PacketWriter();
            packetWriter.Write((byte)Headers.Pause);
            packetWriter.Write(queue.Id);
            Send(packetWriter.GetBytes());
        }

        public int GetOverallProgress()
        {
            int overall = 0;

            if (_transfers != null)
            {
                foreach (var pair in _transfers)
                {
                    overall += pair.Value.Progress;
                }

                if (overall > 0)
                {
                    overall = (overall) / (_transfers.Count);
                }
            }

            return overall;
        }

        public void Send(byte[] data)
        {
            if (Closed)
            {
                return;
            }

            lock (this)
            {
                try
                {
                    _baseSocket.Send(BitConverter.GetBytes(data.Length), 0, 4, SocketFlags.None);
                    _baseSocket.Send(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception)
                {
                    Close();
                }
            }
        }

        public void Close()
        {
            Closed = true;
            _baseSocket.Close();

            if (_transfers != null)
            {
                _transfers.Clear();
                _transfers = null;
            }
            _buffer = null;
            OutputFolder = null;

            if (Disconnected!=null)
            {
                Disconnected(this, EventArgs.Empty);
            }
        }

        private void process()
        {
            PacketReader packetReader = new PacketReader(_buffer);

            Headers header = (Headers)packetReader.ReadByte();

            switch (header)
            {
                case Headers.Queue:
                    {
                        int id = packetReader.ReadInt32();
                        string fileName = packetReader.ReadString();
                        long length = packetReader.ReadInt64();

                        TransferQueue queue = TransferQueue.CreateDownloadQueue(this, id, Path.Combine(OutputFolder, Path.GetFileName(fileName)), length);

                        _transfers.Add(id, queue);

                        if (Queued != null)
                        {
                            Queued(this, queue);
                        }
                    }
                    break;
                case Headers.Start:
                    {
                        int id = packetReader.ReadInt32();

                        if (_transfers.ContainsKey(id))
                        {
                            _transfers[id].Start();
                        }
                    }
                    break;
                case Headers.Stop:
                    {
                        int id = packetReader.ReadInt32();

                        if (_transfers.ContainsKey(id))
                        {
                            TransferQueue queue = _transfers[id];
                            queue.Stop();
                            queue.Close();

                            if (Stopped !=null)
                            {
                                Stopped(this, queue);
                            }

                            _transfers.Remove(id);
                        }
                    }
                    break;
                case Headers.Pause:
                    {
                        int id = packetReader.ReadInt32();

                        if (_transfers.ContainsKey(id))
                        {
                            TransferQueue queue = _transfers[id];

                            if (queue.Type == QueueType.Upload)
                            {
                                queue.Pause();
                            }

                            if (Paused != null)
                            {
                                Paused(this, queue);
                            }
                        }
                    }
                    break;
                case Headers.Chunk:
                    {
                        int id = packetReader.ReadInt32();
                        long index = packetReader.ReadInt64();
                        int size = packetReader.ReadInt32();
                        byte[] buffer = packetReader.ReadBytes(size);

                        TransferQueue queue = _transfers[id];

                        queue.Write(buffer, index);
                        queue.Progress = (int)((queue.Transferred * 100) / queue.Length);

                        if (queue.LastProgress<queue.Progress)
                        {
                            queue.LastProgress = queue.Progress;

                            if (ProgressChanged!=null)
                            {
                                ProgressChanged(this, queue);
                            }

                            if (queue.Progress==100)
                            {
                                queue.Close();

                                if (Complete!=null)
                                {
                                    Complete(this, queue);
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            packetReader.Dispose();
        }

        private void receiveCallback(IAsyncResult asyncResult)
        {
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
            catch (Exception)
            {
                Close();
            }
        }

        internal void callProgressChanged(TransferQueue queue)
        {
            if (ProgressChanged!=null)
            {
                ProgressChanged(this, queue);
            }
        }

        internal void callCompleted(TransferQueue queue)
        {
            if (Complete != null)
            {
                Complete(this, queue);
            }
        }
    }
}
