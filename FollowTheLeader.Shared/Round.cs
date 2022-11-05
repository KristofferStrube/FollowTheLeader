namespace FollowTheLeader.Shared;

public class Round
{
    public const decimal StartTime = 20;
    public decimal Time { get; set; }
    public Dictionary<string, int> Points { get; set; } = new();
    public string Leader { get; set; } = "";
}
