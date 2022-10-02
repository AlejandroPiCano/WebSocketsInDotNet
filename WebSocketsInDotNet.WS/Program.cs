using System.Net.Mime;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };

app.UseWebSockets(wsOptions);
app.Use(async (HttpContext context, Func<Task> next) =>
{
    if (context.Request.Path == "/send")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
            {
                await Send(context, webSocket);
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
});
app.MapGet("/", () => "Hello World!");

app.Run();


async Task Send(HttpContext context, WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    if (result != null)
    {
        while (!result.CloseStatus.HasValue)
        {
            string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));
            Console.WriteLine($"client says: {msg}");
            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Server says: {DateTime.UtcNow:f}")),WebSocketMessageType.Text, result.EndOfMessage, CancellationToken.None);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
    }
    await webSocket.CloseAsync(result != null ? result.CloseStatus?? WebSocketCloseStatus.NormalClosure : WebSocketCloseStatus.NormalClosure, result != null ? result.CloseStatusDescription : string.Empty, CancellationToken.None); ;
  
}