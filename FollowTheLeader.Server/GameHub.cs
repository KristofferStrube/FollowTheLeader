using FollowTheLeader.Server.Infrastructure;
using FollowTheLeader.Shared;
using Microsoft.AspNetCore.SignalR;

namespace FollowTheLeader.Server;

public class GameHub : Hub
{
    public async Task Join()
    {
        Player newPlayer = new()
        {
            ConnectionID = Context.ConnectionId,
            Color = $"#{Random.Shared.Next(999)}"
        };
        StaticStorage.Games.First().Players.Add(newPlayer);
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    public async Task StartGame()
    {
        StaticStorage.Games.First().IsStarted = true;
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());

        for (int i = 0; i < 0; i++)
        {
            await UpdateHeadingsAsync();
            Move();
            await Clients.All.SendAsync("Update", StaticStorage.Games.First());
            await Task.Delay(100);
        }
    }

    private async Task UpdateHeadingsAsync()
    {
        var directions = await Task.WhenAll(StaticStorage.Games.First().Players.Select(async p => await Clients.Client(p.ConnectionID).InvokeAsync<int>("Direction", CancellationToken.None)));
        for (int j = 0; j < directions.Length; j++)
        {
            StaticStorage.Games.First().Players[j].Heading += directions[j] / 10.0;
        }
    }

    private void Move()
    {
        foreach(var player in StaticStorage.Games.First().Players)
        {
            player.Position = ((int)(player.Position.X + Math.Sin(player.Heading) * player.Speed), (int)(player.Position.Y + Math.Cos(player.Heading) * player.Speed));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        StaticStorage.Games.First().Players.Remove(
            StaticStorage.Games.First().Players
                .Where(p => p.ConnectionID == Context.ConnectionId)
                .Single()
            );
    }
}
