using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class GameHub : Hub
{
    private static int playerCount = 0;
    private static readonly object _lock = new object();
    private static ConcurrentDictionary<string, string> playerConnections = new ConcurrentDictionary<string, string>();

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        lock (_lock)
        {
            playerCount--;
            playerConnections.TryRemove(Context.ConnectionId, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGame()
    {
        lock (_lock)
        {
            playerCount++;
            playerConnections[Context.ConnectionId] = Context.ConnectionId;
        }

        if (playerCount == 1)
        {
            await Clients.Caller.SendAsync("WaitingForOpponent");
        }
        else if (playerCount == 2)
        {
            await Clients.All.SendAsync("StartGame");
        }
        else
        {
            await Clients.Caller.SendAsync("GameFull");
        }
    }

    public async Task PlaceShip(int x, int y, string orientation)
    {
        await Clients.Others.SendAsync("ShipPlaced", x, y, orientation);
    }
}
