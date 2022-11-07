using FollowTheLeader.Server.Infrastructure;
using FollowTheLeader.Shared;
using Microsoft.AspNetCore.SignalR;
using Faker;
using Faker.Resources;
using System.Security.Principal;

namespace FollowTheLeader.Server;

public class GameHub : Hub
{
    public static List<(double, double)> frequenzyPairs = new() {
        (138.6, 277.2),
        (146.8, 293.7),
        (155.6, 311.1),
        (164.8, 329.6)
    };

    public async Task<bool> ReadyToJoin()
    {
        if (StaticStorage.Games.First().Players.Count is 0)
        {
            StaticStorage.Games.Clear();
            StaticStorage.Games.Add(new() { Seed = Random.Shared.NextDouble() });
        }
        return StaticStorage.Games.First().Rounds.Sum(r => r.Time) <= 0;
    }

    public async Task Join()
    {
        var sounds = frequenzyPairs[Random.Shared.Next(frequenzyPairs.Count)];
        Player newPlayer = new()
        {
            Name = Faker.Name.First(),
            ConnectionID = Context.ConnectionId,
            Color = $"hsl({Random.Shared.Next(360)}deg 70% 70%)",
            CollideSoundFrequenzy = sounds.Item1,
            MoveSoundFrequenzy = sounds.Item2,
        };
        newPlayer.Body.Enqueue(newPlayer.Head);
        StaticStorage.Games.First().Players.Add(newPlayer);
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    public void Direction(int direction)
    {
        var player = StaticStorage.Games.First().Players.FirstOrDefault(p => p.ConnectionID == Context.ConnectionId);
        if (player is null) return;
        player.Heading += direction / 20.0;
    }

    public void Speed(int speed)
    {
        var player = StaticStorage.Games.First().Players.FirstOrDefault(p => p.ConnectionID == Context.ConnectionId);
        if (player is null) return;

        if (StaticStorage.Games.First().Rounds.FirstOrDefault(r => r.Time > 0)?.Leader == Context.ConnectionId)
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
        StaticStorage.Games.First().Starter = Context.ConnectionId;
        for (int i = 0; i < StaticStorage.Games.First().Players.Count * 2; i++)
        {
            StaticStorage.Games.First().Rounds.Add(new()
            {
                Leader = StaticStorage.Games.First().Players[i % StaticStorage.Games.First().Players.Count].ConnectionID,
                Points = StaticStorage.Games.First().Players.ToDictionary(p => p.ConnectionID, _ => 0),
                Time = Round.StartTime,
            });
        }
        foreach (var player in StaticStorage.Games.First().Players)
        {
            player.Head = new() { X = 50 + Random.Shared.NextDouble(), Y = 50 + Random.Shared.NextDouble() };
        }
        StaticStorage.Games.First().ScoreBoard = null;
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    public async Task Iterate()
    {
        var round = StaticStorage.Games.First().Rounds.FirstOrDefault(r => r.Time > 0);
        if (round is null) return;
        Move();
        await Collide();
        GivePoints();
        round.Time -= 0.020m;
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }

    public async Task Stop()
    {
        var game = StaticStorage.Games.First();
        game.IsStarted = false;
        game.Starter = "";
        game.ScoreBoard = game.Players.ToDictionary(p => p.ConnectionID, p => game.Rounds.Sum(r => r.Points[p.ConnectionID]));
        game.Rounds = new List<Round>();
        await Clients.All.SendAsync("Update", game);
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

    private async Task Collide()
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
                        await Clients.Client(secondary.ConnectionID).SendAsync("Sound", primary.CollideSoundFrequenzy, 1);
                        await Clients.Client(primary.ConnectionID).SendAsync("Sound", secondary.CollideSoundFrequenzy, 1);
                        MovePlayer(primary);
                    }
                }
            }
        }
    }

    private void GivePoints()
    {
        var round = StaticStorage.Games.First().Rounds.FirstOrDefault(r => r.Time > 0);
        if (round is null) return;
        var players = StaticStorage.Games.First().Players;
        var leader = players.First(p => p.ConnectionID == round.Leader);
        var largestDist = 0;
        foreach (var player in players)
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

    public async Task ReRoll()
    {
        var game = StaticStorage.Games.First();
        var player = game.Players.First(p => p.ConnectionID == Context.ConnectionId);
        player.Name = Faker.Name.First();
        player.Decoration = Player.Decorations[Random.Shared.Next(Player.Decorations.Count())];
        player.DecorationFill = Player.Colors[Random.Shared.Next(Player.Colors.Count())];
        player.Color = $"hsl({Random.Shared.Next(360)}deg 70% 70%)";
        await Clients.All.SendAsync("Update", game);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        var game = StaticStorage.Games.First();
        if (game.Players.Where(p => p.ConnectionID == Context.ConnectionId).Count() is 0) return;
        game.Players.Remove(
            game.Players
                .Where(p => p.ConnectionID == Context.ConnectionId)
                .Single()
            );
        game.Rounds = game.Rounds.Where(r => r.Leader != Context.ConnectionId).ToList();
        game.ScoreBoard = game.ScoreBoard?.Where(s => s.Key != Context.ConnectionId).ToDictionary(s => s.Key, s => s.Value);
        if (game.Starter == Context.ConnectionId && game.Players.Count > 0)
        {
            game.Starter = game.Players.First().ConnectionID;
        }
        await Clients.All.SendAsync("Update", StaticStorage.Games.First());
    }
}
