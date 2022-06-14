using System.Net;
using System.Net.Sockets;

namespace MIPT.VRM.Sockets
{
    public class SocketServer : IDisposable
    {
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly EndPoint endPoint;

        public EventHandler<SocketArgs> ClientConnected;

        public SocketServer(EndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public void Start()
        {
            this.socket.NoDelay = true;
            this.socket.Bind(this.endPoint);
            this.socket.Listen(10);

            this.AcceptAsync();
        }

        public void Stop()
        {
            this.socket.Close();
        }

        public void Dispose()
        {
            this.socket.Dispose();
        }

        private void AcceptAsync()
        {
            var acceptArgs = new SocketAsyncEventArgs();
            acceptArgs.Completed += this.AcceptOnCompleted;

            try
            {
                var async = this.socket.AcceptAsync(acceptArgs);

                if (!async)
                    this.AcceptOnCompleted(this.socket, acceptArgs);
            }
            catch (Exception e)
            {
                acceptArgs.Completed -= this.AcceptOnCompleted;
                this.socket.Close();
            }
        }

        private void AcceptOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= this.AcceptOnCompleted;

            if (args.SocketError != SocketError.Success)
                return;

            var client = args.AcceptSocket;

            this.ClientConnected?.Invoke(this, new SocketArgs(new SocketClient(client)));

            this.AcceptAsync();
        }
    }
}
