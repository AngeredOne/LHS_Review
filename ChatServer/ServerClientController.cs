using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ServerClientController
{

    const String helloMessage = "Welcome to chat!\n\rWhat is your name?\n\r";

    private ServerClientModel c_model { get; set; }


    public ServerClientController(TcpClient client)
    {
        c_model = new ServerClientModel(0, client);
    }

    public async void HandleClient(Action<ServerClientController, String> messageCallback, Action<ServerClientController> disconnectCallback)
    {
        SendMessage(helloMessage);
        String nameAnswer = ReadMessage();

        c_model.SetName(nameAnswer);

        while (c_model.client.Connected)
        {
            String receiveMessage = ReadMessage();
            await Task.Run(() => messageCallback(this, receiveMessage));
        }

        disconnectCallback(this);

    }

    private String ReadMessage()
    {
        String message = "";
        while (message.IndexOf("<EOF>") == -1)
        {
            byte[] buffer = new byte[1024 * 2];
            int byteReadCount = 0;
            try
            {
                byteReadCount = c_model.stream.Read(buffer, 0, 1024 * 2);
            }
            catch
            {

            }

            message += Encoding.Unicode.GetString(buffer, 0, byteReadCount);
        }
        return message.Replace("<EOF>", "");
    }

    public void SendMessage(String message)
    {
        if (!c_model.client.Connected) return;

        String _message = message + "<EOF>";
        byte[] msg_bytes = Encoding.Unicode.GetBytes(_message);
        c_model.stream.Write(msg_bytes, 0, _message.Length * 2);

    }

    public String GetClientName()
    { return c_model.name; }

    public void Disconnect()
    {
        c_model.client.Client.Disconnect(true);
    }

}