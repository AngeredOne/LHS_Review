using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
public class Client
{
    TcpClient myClient;
    public async void ClientEntry()
    {
        myClient = new TcpClient("127.0.0.1", 25565);
        Console.WriteLine("Connecting to server");

        await Task.Run(() => ReadMessages());
    }

    public void ReadInputs()
    {
        while (myClient.Connected)
        {
            String input = Console.ReadLine();
            SendMessage(input + "<EOF>");
        }
    }

    public void ReadMessages()
    {
        while (myClient.Connected)
        {
            String message = "";

            while (message.IndexOf("<EOF>") == -1)
            {
                byte[] buffer = new byte[16];
                try
                {
                    myClient.GetStream().Read(buffer, 0, 16);
                }
                catch
                {

                }

                message += Encoding.UTF8.GetString(buffer);
            }

            Console.WriteLine(message.Replace("<EOF>", ""));
        }
    }

    public void SendMessage(String message)
    {
        byte[] msg_bytes = Encoding.UTF8.GetBytes(message);
        myClient.GetStream().Write(msg_bytes, 0, message.Length);
    }
}