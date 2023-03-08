using SocketsSSLServer;

void runServer(int socketNo = 32000, string host = "localhost")
{
    if (args.Length > 1)
    {
        host = args[1];
    }

    if (args.Length > 2)
    {
        socketNo = Int16.Parse(args[1]);
    }

    SocketServer server = new SocketServer(host, socketNo);
    server.Initialize();
    server.AcceptsRequests();
    server.Close();
}

runServer();
Console.WriteLine("Application exited!");

