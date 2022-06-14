using System.Net;
using MIPT.VRM.Common.Entities;
using MIPT.VRM.Common.Serialization;
using MIPT.VRM.Sockets;
using OpenTK.Mathematics;
using States = System.Collections.Concurrent.ConcurrentDictionary<string, MIPT.VRM.Common.Entities.VrmObjectState>;

namespace MIPT.VRM.Server;

public class AppServer
{
    private List<VrmObjectState> clientState = new ()
    {
        new VrmObjectState(1, Matrix4.Identity + Matrix4.CreateTranslation(-2 * Vector3.UnitX), 1.0f),
        //new VrmObjectState(2, Matrix4.Identity + Matrix4.CreateTranslation(3 * Vector3.UnitX), 1.5f),
    };

    
    private static readonly EndPoint endPoint = new IPEndPoint(IPAddress.Any, 9999);
    private static readonly VrmFormatter formatter = new VrmFormatter();
    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
    private readonly SocketServer server = new SocketServer(endPoint);
    private readonly List<SocketClient> clients = new List<SocketClient>();
    private readonly States states = new States();
    private readonly Thread watchDog;

    public AppServer()
    {
        this.watchDog = new Thread(this.WorkingThread);
        this.server.ClientConnected += this.ClientConnected;
    }

    public void Start()
    {
        this.watchDog.Start();
        this.server.Start();

        foreach (var objectState in this.clientState)
            this.states.TryAdd((this.states.Count + 1).ToString(), objectState);
    }

    private void WorkingThread()
    {
        while (true)
        {
            var states = this.states.Values.ToArray();
            var message = formatter.WriteStates(states);
        
            this.locker.EnterReadLock();
            this.clients.ForEach(x=>x.SendAsync(message));
            this.locker.ExitReadLock();
        
            Thread.Sleep(200);
        }
    }

    private void ClientConnected(object? sender, SocketArgs e)
    {
        var client = e.SocketClient;
        client.StatusChanged += this.StatusChanged;
        client.MessageReceived += this.MessageReceived;
        client.MessageSent += this.MessageSent;
        client.ReceiveAsync();
        
        this.locker.EnterWriteLock();
        this.clients.Add(client);
        this.locker.ExitWriteLock();

        var key = e.SocketClient.RemoteEndpoint;
        var counts = this.states.Count + 1;
        var state = new VrmObjectState(counts, Matrix4.Identity + Matrix4.CreateTranslation( (2 * counts) * Vector3.UnitX), 1);
        this.states.TryAdd(key.ToString(), state);
    }

    private void StatusChanged(object? sender, SocketStatusArgs e)
    {
        if (!e.IsConnected)
        {
            e.SocketClient.StatusChanged -= this.StatusChanged;
            e.SocketClient.MessageReceived -= this.MessageReceived;
            e.SocketClient.MessageSent -= this.MessageSent;
        }
        
        this.locker.EnterWriteLock();
        this.clients.Remove(e.SocketClient);
        this.locker.ExitWriteLock();
    }

    private void MessageReceived(object? sender, SocketMessageArgs e)
    {
        var command = formatter.ReadCommand(e.Message);
        var key = e.SocketClient.RemoteEndpoint;

        if (this.states.TryGetValue(key.ToString(), out var state))
            state.Update(command.Coord);
    }

    private void MessageSent(object? sender, SocketMessageArgs e)
    {
        Console.WriteLine($"Server sent {e.Message.Length} bytes to {e.SocketClient.RemoteEndpoint}");
    }
}
