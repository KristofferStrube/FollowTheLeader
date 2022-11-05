namespace FollowTheLeader.Shared;

public class Player
{
    public string ConnectionID { get; set; } = "";
    public string Color { get; set; } = "black";
    public double Heading { get; set; } = 45;
    public double Speed { get; set; } = 2;
    public Queue<Position> Body { get; set; } = new();
    public Position Head { get; set; } = new() { X = 50, Y = 50 };
}
