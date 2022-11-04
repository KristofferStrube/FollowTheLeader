namespace FollowTheLeader.Shared;

public class Player
{
    public string ConnectionID { get; set; } = "";
    public string Color { get; set; } = "black";
    public (int X, int Y) Position { get; set; } = (50, 50);
    public double Heading { get; set; }
    public double Speed { get; set; } = 10;
}
