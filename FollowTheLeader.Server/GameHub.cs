using FollowTheLeader.Server.Infrastructure;
using FollowTheLeader.Shared;
using Microsoft.AspNetCore.SignalR;
using Faker;

namespace FollowTheLeader.Server;

public class GameHub : Hub
{
    public async Task Join()
    {
        Player newPlayer = new()
        {
            Name = Faker.Name.First(),
            ConnectionID = Context.ConnectionId,
            Color = $"hsl({Random.Shared.Next(360)}deg 70% 70%)",
        };
        newPlayer.Body.Enqueue(newPlayer.Head);
        StaticStorage.Games.First().Players.Add(newPlayer);
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    public void Direction(int direction)
    {
        var player = StaticStorage.Games.First().Players.First(p => p.ConnectionID == Context.ConnectionId);
        player.Heading += direction / 20.0;
    }

    public void Speed(int speed)
    {
        var player = StaticStorage.Games.First().Players.First(p => p.ConnectionID == Context.ConnectionId);

        if (StaticStorage.Games.First().Rounds.First(r => r.Time > 0).Leader == Context.ConnectionId)
        {
            player.Speed = 2;
        }
        else
        {
            player.Speed = 2 + speed;
        }
    }

    public async Task StartGame()
    {
        StaticStorage.Games.First().IsStarted = true;
        for(int i = 0; i < StaticStorage.Games.First().Players.Count * 2; i++)
        {
            StaticStorage.Games.First().Rounds.Add(new()
            {
                Leader = StaticStorage.Games.First().Players[i % StaticStorage.Games.First().Players.Count].ConnectionID,
                Points = StaticStorage.Games.First().Players.ToDictionary(p => p.ConnectionID, _ => 0)
            });
        }
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());

        var time = DateTime.Now;
        while (StaticStorage.Games.First().Rounds.Any(r => r.Time > 0))
        {
            Move();
            Collide();
            GivePoints();
            var diff = (int)(DateTime.Now - time).TotalMilliseconds;
            if (diff < 20)
            {
                await Task.Delay(20 - diff);
                StaticStorage.Games.First().Rounds.First(r => r.Time > 0).Time -= 0.020m;
            }
            else
            {
                StaticStorage.Games.First().Rounds.First(r => r.Time > 0).Time -= diff / 1000.0m;
            }
            time = DateTime.Now;
            await Clients.All.SendAsync("Update", StaticStorage.Games.First());
        }
        StaticStorage.Games.First().IsStarted = false;
        StaticStorage.Games.First().ScoreBoard = StaticStorage.Games.First().Players.ToDictionary(p => p.ConnectionID, p => StaticStorage.Games.First().Rounds.Sum(r => r.Points[p.ConnectionID]));

        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    private void Move()
    {
        foreach (var player in StaticStorage.Games.First().Players)
        {
            MovePlayer(player);
        }
    }

    private void MovePlayer(Player player)
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

    private void Collide()
    {
        foreach (var primary in StaticStorage.Games.First().Players)
        {
            foreach (var secondary in StaticStorage.Games.First().Players)
            {
                if (primary == secondary || StaticStorage.Games.First().Rounds.First(r => r.Time > 0).Leader == primary.ConnectionID) continue;
                foreach (var part in secondary.Body)
                {
                    double dist = Math.Sqrt(Math.Pow(primary.Head.X - part.X, 2) + Math.Pow(primary.Head.Y - part.Y, 2));
                    if (dist < 3)
                    {
                        primary.Heading = Math.Atan((primary.Head.X - part.X) / (primary.Head.Y - part.Y));
                        primary.Speed = 10;
                        MovePlayer(primary);
                    }
                }
            }
        }
    }

    private void GivePoints()
    {
        var round = StaticStorage.Games.First().Rounds.First(r => r.Time > 0);
        var players = StaticStorage.Games.First().Players;
        var leader = players.First(p => p.ConnectionID == round.Leader);
        var largestDist = 0;
        foreach(var player in players)
        {
            var dist = (int)Math.Sqrt(Math.Pow(player.Head.X - leader.Head.X, 2) + Math.Pow(player.Head.Y - leader.Head.Y, 2));
            largestDist = dist > largestDist ? dist : largestDist;
            round.Points[player.ConnectionID] -= dist;
        }
        foreach (var player in players)
        {
            round.Points[player.ConnectionID] += largestDist;
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
