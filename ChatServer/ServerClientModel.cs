using System;
using System.Net.Sockets;

public class ServerClientModel
{

    public Int64 uid { get; private set; }
    public String name { get; private set; }

    public TcpClient client { get; private set; }
    public NetworkStream stream { get; private set; }

    public ServerClientModel(Int64 _uid, TcpClient _client)
    {
        uid = _uid;
        client = _client;
        stream = client.GetStream();
    }

    public void SetName(String _name)
    {
        name = _name;
    }

}