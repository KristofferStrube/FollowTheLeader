namespace FollowTheLeader.Shared;

public class Round
{
    public decimal Time { get; set; } = 20;
    public Dictionary<string, int> Points { get; set; } = new();
    public string Leader { get; set; } = "";
}
