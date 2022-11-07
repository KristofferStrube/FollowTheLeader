namespace FollowTheLeader.Shared;

public class Game
{
    public List<Player> Players { get; set; } = new();
    public bool IsStarted { get; set; }
    public string Starter { get; set; }
    public DateTime LastIterate { get; set; }
    public double Seed { get; set; }
    public List<Round> Rounds { get; set; } = new();
    public Dictionary<string, int>? ScoreBoard { get; set; }
}
