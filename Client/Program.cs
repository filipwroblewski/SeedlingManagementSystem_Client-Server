using SocketsSSLClient;

void connectToServer()
{
    int socketNo = 32000;
    string host = "localhost";

    if (args.Length > 1)
    {
        host = args[1];
    }

    if (args.Length > 2)
    {
        socketNo = Int16.Parse(args[1]);
    }
    SocketClient client = new SocketClient(host, socketNo);
    client.Connect();
}
Console.WriteLine("Hello, Client!");
Console.WriteLine(@"
to see what you can do type (when you switch between accounts it may be changed): whatToDo
");
connectToServer();
Console.WriteLine("Application exited!");