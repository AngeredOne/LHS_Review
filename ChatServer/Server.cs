using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

public class Server
{

    private delegate void commandHandlerDelegate(ServerClientController controller);
    private Dictionary<String, commandHandlerDelegate> commands = new Dictionary<String, commandHandlerDelegate>();

    private static ManualResetEvent clientConnectionReady = new ManualResetEvent(false);
    private static Object run_lock = new Object();
    private static Object clients_lock = new Object();

    private TcpListener acceptor { get; set; }
    private List<ServerClientController> clients { get; set; } = new List<ServerClientController>();

    public int port { get; private set; }

    public bool isRunning { get; private set; } = false;
    public bool clientsCount { get; private set; } = false;


    public void SetPort(int _port)
    {
        if (!isRunning)
        {
            port = _port;
        }
        else
        {
            throw new Exception("Trying to set port while server running!");
        }
    }

    public void InitializeCommands()
    {
        commands.Add("как дела", (ServerClientController scc) => scc.SendMessage("Хорошо!"));
        commands.Add("пока", GoodByeCommandHandler);
        commands.Add("список", (ServerClientController scc) => scc.SendMessage("Список подключенных клиентов: "
                                                               + String.Join(';', clients.Select(x => x.GetClientName()))));
    }

    public void CreateAcceptor()
    {
        acceptor = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
    }

    public async void Run()
    {
        if (!isRunning)
        {
            acceptor.Start();
            Console.WriteLine("Server started.\n---> Port: " + port);

            await Task.Run(() => AcceptorLoop());
        }
        else
        {
            throw new Exception("Server already running");
        }
    }

    private void AcceptorLoop()
    {
        lock (run_lock)
        {
            isRunning = true;
        }
        while (isRunning)
        {
            clientConnectionReady.Reset();

            acceptor.BeginAcceptTcpClient(new AsyncCallback(HandleClientConnection), acceptor);

            clientConnectionReady.WaitOne();
        }
    }

    private void HandleClientConnection(IAsyncResult asyres)
    {
        TcpListener acceptorRef = (TcpListener)asyres.AsyncState;
        TcpClient client = acceptorRef.EndAcceptTcpClient(asyres);
        clientConnectionReady.Set();

        ServerClientController scc = new ServerClientController(client);

        clients.Add(scc);
        scc.HandleClient(HandleClientMessage, HandleClientDisconnect);

    }

    private void HandleClientDisconnect(ServerClientController clientController)
    {
        lock (clients_lock)
        {
            clients.Remove(clientController);
        }

        BroadCastMessage(clientController, $"<<< {clientController.GetClientName()} has leaved the chat!");
        Console.WriteLine($"{clientController.GetClientName()}: DISCONNECTED!");
    }

    private void HandleClientMessage(ServerClientController clientController, String message)
    {

        String command = new String(message.Where(c => !char.IsPunctuation(c) && !char.IsSymbol(c)).ToArray()).ToLower();

        commandHandlerDelegate commandToExecute = commands.GetValueOrDefault(command, null);
        if (commandToExecute != null)
        {
            commandToExecute(clientController);
            return;
        }

        String sendMessage = $"{clientController.GetClientName()} says: {message}";
        BroadCastMessage(clientController, sendMessage);
        // Console.WriteLine(sendMessage);

    }

    private void BroadCastMessage(ServerClientController clientController, String message)
    {
        clients.Where(x => !x.Equals(clientController)).ToList().ForEach(controller =>
                    {
                        controller.SendMessage(message);
                    }
                );
    }


    #region CommandHandlers

    private void GoodByeCommandHandler(ServerClientController controller)
    {
        controller.SendMessage("Досвидания!");
        controller.Disconnect();
    }

    #endregion


}