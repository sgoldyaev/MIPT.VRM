using System.Net;
using System.Net.Sockets;

namespace MIPT.VRM.Sockets
{
    public class SocketClient : IDisposable
    {
        private readonly Socket socket;
        private bool isConnected = false;

        public EndPoint RemoteEndpoint => this.socket.RemoteEndPoint;
        public EndPoint LocalEndpoint => this.socket.LocalEndPoint;

        public bool IsConnected
        {
            get => this.isConnected;
            private set
            {
                if (value == this.isConnected) return;

                this.isConnected = value;
                this.StatusChanged(this, new SocketStatusArgs(this, this.isConnected));
            }
        }

        public EventHandler<SocketStatusArgs> StatusChanged;
        public EventHandler<SocketMessageArgs> MessageReceived;
        public EventHandler<SocketMessageArgs> MessageSent;

        public SocketClient(Socket socket)
        {
            this.socket = socket;
            this.socket.NoDelay = true;
            this.isConnected = true;
        }

        public SocketClient() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            this.isConnected = false;
        }

        public void Dispose()
        {
            this.socket.Dispose();
        }

        public void ConnectAsync(EndPoint address)
        {
            if (this.isConnected) return;

            var connectArgs = new SocketAsyncEventArgs();
            connectArgs.RemoteEndPoint = address;
            connectArgs.Completed += this.ConnectOnCompleted;

            try
            {
                var async = this.socket.ConnectAsync(connectArgs);

                if (!async)
                    this.ConnectOnCompleted(this, connectArgs);
            }
            catch (Exception e)
            {
                connectArgs.Completed -= this.ConnectOnCompleted;
                this.IsConnected = false;
            }
        }

        private void ConnectOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= this.ConnectOnCompleted;

            if (args.SocketError != SocketError.Success)
                return;

            this.IsConnected = true;
        }

        public void SendAsync(params byte[] data)
        {
            if (!this.isConnected) return;

            var bufferList = new List<ArraySegment<byte>>
            {
                new ArraySegment<byte>(BitConverter.GetBytes(data.Length)),
                new ArraySegment<byte>(data)
            };
            var sendArgs = new SocketAsyncEventArgs();
            sendArgs.BufferList = bufferList;
            sendArgs.Completed += this.SendOnCompleted;

            try
            {
                var async = this.socket.SendAsync(sendArgs);

                if (!async)
                    this.SendOnCompleted(this, sendArgs);
            }
            catch (Exception e)
            {
                sendArgs.Completed -= this.SendOnCompleted;
                this.socket.Close();
                this.IsConnected = false;
            }
        }

        private void SendOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= this.SendOnCompleted;

            if (args.SocketError != SocketError.Success)
                return;

            var message = args.BufferList[1].Array;

            this.MessageSent?.Invoke(this, new SocketMessageArgs(this, message));
        }

        public void ReceiveAsync()
        {
            if (!this.isConnected) return;

            var buffer = new byte[4];
            var receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.SetBuffer(buffer, 0, buffer.Length);
            receiveArgs.Completed += this.ReceiveHeaderOnCompleted;

            try
            {
                var async = this.socket.ReceiveAsync(receiveArgs);

                if (!async)
                    this.ReceiveHeaderOnCompleted(this, receiveArgs);
            }
            catch (Exception e)
            {
                receiveArgs.Completed -= this.ReceiveHeaderOnCompleted;
                this.socket.Close();
                this.IsConnected = false;
            }
        }

        private void ReceiveHeaderOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= this.ReceiveHeaderOnCompleted;

            if (args.SocketError != SocketError.Success || !this.isConnected)
                return;

            var messageSize = BitConverter.ToInt32(args.Buffer, 0);

            Console.WriteLine($"Receiving {messageSize} bytes from {this.socket.RemoteEndPoint}");

            if (messageSize < 1)
                return;

            var receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.SetBuffer(new byte[messageSize], 0, messageSize);
            receiveArgs.Completed += this.ReceiveBodyOnCompleted;

            try
            {
                var async = this.socket.ReceiveAsync(receiveArgs);

                if (!async)
                    this.ReceiveBodyOnCompleted(this, receiveArgs);
            }
            catch (Exception e)
            {
                receiveArgs.Completed -= this.ReceiveBodyOnCompleted;
                this.socket.Close();
                this.IsConnected = false;
            }
        }

        private void ReceiveBodyOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= this.ReceiveBodyOnCompleted;

            if (args.BytesTransferred < args.Count)
            {
                if (args.SocketError != SocketError.Success)
                    return;

                var offset = args.Offset + args.BytesTransferred;
                var count = args.Count - args.BytesTransferred;

                args.SetBuffer(offset, count);
                args.Completed += this.ReceiveBodyOnCompleted;

                try
                {
                    var async = this.socket.ReceiveAsync(args);

                    if (!async)
                        this.ReceiveBodyOnCompleted(this, args);
                }
                catch (Exception e)
                {
                    args.Completed -= this.ReceiveBodyOnCompleted;
                    this.socket.Close();
                    this.IsConnected = false;
                }
            }
            else
            {
                this.MessageReceived?.Invoke(this, new SocketMessageArgs(this, args.Buffer));

                this.ReceiveAsync();
            }
        }
    }
}
