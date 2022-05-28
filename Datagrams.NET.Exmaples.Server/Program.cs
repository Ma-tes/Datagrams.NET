using System.Net;

namespace Datagrams.NET.Examples.Server 
{
    internal class Program 
    {
        private static ServerExample server;

        public static void Main() 
        {
            string ipAddress = Console.ReadLine();
            server = new ServerExample("ServerExample", IPAddress.Parse("10.0.0.12"));
            Task.Run(() => server.StartServer());
            Console.ReadLine();
            Console.WriteLine(server.handShakeCounter);
            Console.ReadLine();
        }
    }
}
