using FollowTheLeader.Shared;

namespace FollowTheLeader.Server.Infrastructure;

public static class StaticStorage
{
    public static List<Game> Games = new() { new Game() { Seed = Random.Shared.NextDouble() } };
}
