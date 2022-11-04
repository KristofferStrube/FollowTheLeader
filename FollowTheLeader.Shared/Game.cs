namespace FollowTheLeader.Shared;

public class Game
{
    public List<Player> Players { get; set; } = new();
    public bool IsStarted { get; set; }
}
