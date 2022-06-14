namespace MIPT.VRM.Sockets
{
    public class SocketArgs : EventArgs
    {
        public readonly SocketClient SocketClient;

        public SocketArgs(SocketClient socketClient)
        {
            this.SocketClient = socketClient;
        }
    }
}
