﻿using DatagramsNet.Examples.Server;
using DatagramsNet.Logging.Reading;
using System.Net;

var server = new ServerExample(IPAddress.Parse("10.0.0.10"));
//server.CancellationFunction = () => server.handShakeCounter > 5;

Task.Run(() => server.StartServerAsync());
ReaderManager.StartReading();
Console.WriteLine("Listening for clients...");
Console.WriteLine("Press any key to stop");
Console.ReadKey(intercept: true);
Console.WriteLine($"Handshake counter: {server.handShakeCounter}");
