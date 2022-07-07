using Datagrams.NET.Examples.Server;
using DatagramsNet.Datagrams.NET.Logger.Reader;
using System.Net;

var server = new ServerExample("ServerExample", IPAddress.Any);
var consoleReader = new ReaderManager();

Task.Run(() => server.StartServer());
consoleReader.StartReading();
Console.WriteLine("Listening for clients...");
Console.WriteLine("Press any key to stop");
Console.ReadKey(intercept: true);
Console.WriteLine($"Handshake counter: {server.handShakeCounter}");
