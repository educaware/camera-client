// See https://aka.ms/new-console-template for more information

using System;
using System.Threading;
using CameraClient;
using Websocket.Client;

var url = new Uri("ws://localhost:8080/api/v1/ws");
var exitEvent = new ManualResetEvent(false);


var client = new WebsocketClient(url);
var camera = new Camera(client);

// Change client settings.
client.ReconnectTimeout = TimeSpan.FromMinutes(20);
client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);

// Subscribe to specific events.
client.MessageReceived.Subscribe(ParseMessage);

// Start and keep the connection alive.
await client.Start();
exitEvent.WaitOne();


void ParseMessage(ResponseMessage msg)
{
    switch (msg.ToString())
    {
        case "on":
            camera.TurnOn();
            break;

        case "off":
            camera.TurnOff();
            break;

        case { } s when s.StartsWith("blink"):
            // Parse the response.
            var strings = s.Split(",");
            var repeat = Convert.ToInt32(strings[1]);
            var delay = Convert.ToInt32(strings[2]);

            camera.Blink(repeat, delay);
            break;

        case "close":
            // Exit the program safely.
            Environment.Exit(0);
            break;
    }
}
