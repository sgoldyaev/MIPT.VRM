using System.Net;
using MIPT.VRM.Common.Entities;
using MIPT.VRM.Common.Serialization;
using MIPT.VRM.Sockets;
using OpenTK.Windowing.Desktop;

namespace MIPT.VRM.Client;

public class AppClient : IDisposable
{
    private readonly VrmFormatter formatter = new VrmFormatter();
    private readonly EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 9999);
    private readonly SocketClient client = new SocketClient();
    private readonly VrmWindow game = new VrmWindow(GameWindowSettings.Default, NativeWindowSettings.Default);

    public void Start()
    {
        this.client.StatusChanged += this.StatusChanged;
        this.client.MessageReceived += this.MessageReceived;
        this.client.MessageSent += MessageSent;
        this.client.ConnectAsync(this.endPoint);

        this.game.OnCommand += this.OnCommand;
        this.game.Run();
    }

    private void OnCommand(object? sender, VrmCommand e)
    {
        var message = this.formatter.WriteCommand(e);
        
        this.client.SendAsync(message);
    }

    private void StatusChanged(object? sender, SocketStatusArgs e)
    {
        if (e.IsConnected)
            this.client.ReceiveAsync();
    }

    private void MessageReceived(object? sender, SocketMessageArgs e)
    {
        var state = this.formatter.ReadStates(e.Message);
        this.game.Handle(state);
    }

    private static void MessageSent(object? sender, SocketMessageArgs e)
    {
        Console.WriteLine("");
    }

    public void Dispose()
    {
        this.client.StatusChanged += this.StatusChanged;
        this.client.MessageReceived += this.MessageReceived;
        this.client.MessageSent += MessageSent;
        this.client.Dispose();

        this.game.OnCommand -= this.OnCommand;
        this.game.Dispose();
    }
}
