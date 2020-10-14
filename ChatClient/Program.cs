using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace ChatClient
{
    class Program
    {

        static TcpClient myClient;

        static void Main(string[] args)
        {
            ClientEntry();
            while (myClient.Connected)
            {
                String input = Console.ReadLine();
                SendMessage(input + "<EOF>");
            }
        }


        static async void ClientEntry()
        {
            myClient = new TcpClient("127.0.0.1", 25565);
            Console.WriteLine("Connecting to server");

            await Task.Run(() => ReadMessages());
        }

        static void ReadMessages()
        {
            while (myClient.Connected)
            {
                String message = "";

                while (message.IndexOf("<EOF>") == -1)
                {
                    byte[] buffer = new byte[1024 * 2];
                    int byteReadCount = 0;
                    try
                    {
                        byteReadCount = myClient.GetStream().Read(buffer, 0, 1024 * 2);
                    }
                    catch
                    {

                    }

                    message += Encoding.Unicode.GetString(buffer, 0, byteReadCount);
                }

                Console.WriteLine(message.Replace("<EOF>", ""));
            }
        }

        static void SendMessage(String message)
        {
            if (!myClient.Connected) return;

            byte[] msg_bytes = Encoding.Unicode.GetBytes(message);
            myClient.GetStream().Write(msg_bytes, 0, message.Length * 2);
        }

    }
}
