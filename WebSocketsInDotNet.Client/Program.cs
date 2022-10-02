// See https://aka.ms/new-console-template for more information
using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Press Enter to Continue..");
Console.ReadLine();

using (ClientWebSocket client = new ClientWebSocket())
{
    Uri serviceUrl = new Uri("ws://localhost:5263/send");
    var cancellationTokenSource = new CancellationTokenSource();

    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(180));
	
	try
	{
		await client.ConnectAsync(serviceUrl, cancellationTokenSource.Token);
		var i = 0;
		while (client.State == WebSocketState.Open)
		{
			Console.WriteLine("Enter message to send");
			var messageFromCLient = Console.ReadLine();

			if (!string.IsNullOrEmpty(messageFromCLient))
			{
				var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageFromCLient));
				await client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cancellationTokenSource.Token);

				var responseBuffer = new byte[1024];
				var offset = 0; 
				var packet = 1024;
				
				while (true)
				{
					var bytesReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
					var response = await client.ReceiveAsync(bytesReceived, cancellationTokenSource.Token);

					var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
					Console.WriteLine(responseMessage);

					if(response.EndOfMessage)
						break;
				}
			}
		}
	}
	catch (WebSocketException e)
	{
		Console.WriteLine(e.Message);
	}
}

Console.ReadLine();
