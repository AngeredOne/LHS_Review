using System;
using System.Text;
using CommandLine;


namespace ChatServer
{

    class Opts
    {
        [Option('p', "port", Required = false, Default = 25565)]
        public int port { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Opts>(args).WithParsed<Opts>(CreateServer);
        }

        static void CreateServer(Opts options)
        {
            Console.Write("Configuring server...");

            Server server = new Server();
            server.SetPort(options.port);
            server.InitializeCommands();

            Console.Write("  OK\n");

            Console.WriteLine("Starting server...");

            server.CreateAcceptor();
            server.Run();

            Console.Read();
        }
    }
}
