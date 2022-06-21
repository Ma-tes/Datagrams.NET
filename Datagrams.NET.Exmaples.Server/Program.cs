using DatagramsNet.Datagrams.NET.Logger.Reader;
using System.Net;

namespace Datagrams.NET.Examples.Server 
{
    internal class Program 
    {
        private static ServerExample server;

        public static void Main() 
        {
            string ipAddress = Console.ReadLine();
            var consoleReader = new ReaderManager();

            server = new ServerExample("ServerExample", IPAddress.Parse("10.0.0.12"));
            Task.Run(() => server.StartServer());
            consoleReader.StartReading();

            Console.WriteLine(server.handShakeCounter);
            Console.ReadLine();
        }
    }
}
