namespace FollowTheLeader.Shared;

public class Round
{
    public decimal Time { get; set; } = 5;
    public Dictionary<string, int> Points { get; set; } = new();
    public string Leader { get; set; } = "";
}
