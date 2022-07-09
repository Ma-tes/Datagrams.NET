using DatagramsNet.Examples.Server;
using DatagramsNet.Logging.Reading;
using System.Net;

var server = new ServerExample("ServerExample", IPAddress.Any);
var consoleReader = new ReaderManager();

Task.Run(() => server.StartServerAsync());
consoleReader.StartReading();
Console.WriteLine("Listening for clients...");
Console.WriteLine("Press any key to stop");
Console.ReadKey(intercept: true);
Console.WriteLine($"Handshake counter: {server.HandShakeCounter}");
