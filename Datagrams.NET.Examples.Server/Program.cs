using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Logger.Reader;
using DatagramsNet.Datagrams.NET.Prefixes;
using System.Net;

namespace Datagrams.NET.Examples.Server 
{
    internal class Program 
    {
        private static ServerExample server;

        public static void Main() 
        {
            var consoleReader = new ReaderManager();
            Console.WriteLine("Enter your ip address: ");
            string ipAddress = Console.ReadLine();

            ipAddress = ipAddress == String.Empty ? IPAddress.Any.ToString() : ipAddress;
            server = new ServerExample("ServerExample", IPAddress.Parse(ipAddress));
            ServerLogger.Log<NormalPrefix>($"Server is correctly running on address: {ipAddress}", TimeFormat.HALF);

            Task.Run(() => server.StartServer());
            consoleReader.StartReading();
            Console.ReadLine();
        }
    }
}
