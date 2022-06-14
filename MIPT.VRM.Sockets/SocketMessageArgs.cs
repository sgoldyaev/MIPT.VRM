namespace MIPT.VRM.Sockets
{
    public class SocketMessageArgs : SocketArgs
    {
        public readonly byte[] Message;

        public SocketMessageArgs(SocketClient socketClient, byte[] message) : base(socketClient)
        {
            this.Message = message;
        }
    }
}
