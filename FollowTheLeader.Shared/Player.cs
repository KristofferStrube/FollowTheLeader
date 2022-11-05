namespace FollowTheLeader.Shared;

public class Player
{
    public string Name { get; set; }
    public string ConnectionID { get; set; } = "";
    public string Color { get; set; } = "black";
    public double Heading { get; set; } = 45;
    public double Speed { get; set; } = 2;
    public bool IsLeader { get; set; }
    public double MoveSoundFrequenzy { get; set; }
    public double CollideSoundFrequenzy { get; set; }
    public Queue<Position> Body { get; set; } = new();
    public Position Head { get; set; } = new() { X = 50 + Random.Shared.NextDouble(), Y = 50 + Random.Shared.NextDouble() };
}
