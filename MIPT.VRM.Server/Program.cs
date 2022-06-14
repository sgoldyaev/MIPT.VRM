using MIPT.VRM.Server;

var appServer = new AppServer();
appServer.Start();

Console.WriteLine($"Server Started");
Console.ReadLine();