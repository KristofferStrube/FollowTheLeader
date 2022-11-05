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
            Color = $"#{Random.Shared.Next(999)}",
        };
        newPlayer.Body.Enqueue(newPlayer.Head);
        StaticStorage.Games.First().Players.Add(newPlayer);
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    public async Task StartGame()
    {
        StaticStorage.Games.First().IsStarted = true;
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());

        while(true)
        {
            await UpdateHeadingsAsync();
            await UpdateSpeedAsync();
            Move();
            await Clients.All.SendAsync("Update", StaticStorage.Games.First());
            await Task.Delay(20);
        }
    }

    private async Task UpdateHeadingsAsync()
    {
        var directions = await Task.WhenAll(StaticStorage.Games.First().Players.Select(async p => await Clients.Client(p.ConnectionID).InvokeAsync<int>("Direction", CancellationToken.None)));
        for (int j = 0; j < directions.Length; j++)
        {
            StaticStorage.Games.First().Players[j].Heading += directions[j] / 20.0;
        }
    }

    private async Task UpdateSpeedAsync()
    {
        var speeds = await Task.WhenAll(StaticStorage.Games.First().Players.Select(async p => await Clients.Client(p.ConnectionID).InvokeAsync<int>("Speed", CancellationToken.None)));
        for (int j = 0; j < speeds.Length; j++)
        {
            StaticStorage.Games.First().Players[j].Speed = 2 + speeds[j];
        }
    }

    private void Move()
    {
        foreach (var player in StaticStorage.Games.First().Players)
        {
            if (player.Body.Count > 9)
            {
                player.Body.Dequeue();
            }
            player.Head = new()
            {
                X = player.Head.X + Math.Sin(player.Heading) * player.Speed,
                Y = player.Head.Y + Math.Cos(player.Heading) * player.Speed
            };
            player.Body.Enqueue(player.Head);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        if (StaticStorage.Games.First().Players.Where(p => p.ConnectionID == Context.ConnectionId).Count() is 0) return;
        StaticStorage.Games.First().Players.Remove(
            StaticStorage.Games.First().Players
                .Where(p => p.ConnectionID == Context.ConnectionId)
                .Single()
            );
    }
}
