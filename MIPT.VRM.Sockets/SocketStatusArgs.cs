namespace MIPT.VRM.Sockets
{
    public class SocketStatusArgs : SocketArgs
    {
        public readonly bool IsConnected;

        public SocketStatusArgs(SocketClient socketClient, bool isConnected) : base(socketClient)
        {
            this.IsConnected = isConnected;
        }
    }
}
