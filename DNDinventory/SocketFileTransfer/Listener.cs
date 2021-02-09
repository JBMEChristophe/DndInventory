using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.SocketFileTransfer
{
    internal delegate void SocketAcceptedHandler(object sender, SocketAcceptedEventArgs e);

    internal class SocketAcceptedEventArgs : EventArgs
    {
        public Socket Accepted
        {
            get;
            private set;
        }

        public IPAddress Address
        {
            get;
            private set;
        }

        public IPEndPoint EndPoint
        {
            get;
            private set;
        }

        public SocketAcceptedEventArgs(Socket sck)
        {
            Accepted = sck;
            Address = ((IPEndPoint)sck.RemoteEndPoint).Address;
            EndPoint = (IPEndPoint)sck.RemoteEndPoint;
        }
    }

    internal class Listener
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Variables
        private Socket _socket = null;
        private bool _running = false;
        private int _port = -1;
        #endregion

        #region Properties
        public Socket BaseSocket
        {
            get { return _socket; }
        }

        public bool Running
        {
            get { return _running; }
        }

        public int Port
        {
            get { return _port; }
        }
        #endregion

        public event SocketAcceptedHandler Accepted;

        public Listener()
        {
            logger.Info(">< Listener()");
        }

        public void Start(int port)
        {
            logger.Info($"> Start(port: {port})");
            if (_running)
            {
                logger.Info("Already running");
                logger.Info($"< Start(port: {port})");
                return;
            }

            _port = port;
            _running = true;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            _socket.Listen(100);
            _socket.BeginAccept(acceptCallback, null);
            logger.Info($"< Start(port: {port})");
        }

        public void Stop()
        {
            logger.Info("> Stop()");
            if (!_running)
            {
                logger.Info("Already stopped");
                logger.Info("< Stop()");
                return;
            }

            _running = false;
            _socket.Close();
            logger.Info("< Stop()");
        }

        private void acceptCallback(IAsyncResult ar)
        {
            logger.Info($"> acceptCallback(AsyncResult: {ar})");
            try
            {
                Socket sck = _socket.EndAccept(ar);

                Accepted?.Invoke(this, new SocketAcceptedEventArgs(sck));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Whoeps, something went wrong");
            }

            if (_running)
            {
                _socket.BeginAccept(acceptCallback, null);
            }

            logger.Info($"< acceptCallback(AsyncResult: {ar})");
        }
    }
}
