using System.Text.Json.Serialization;

namespace FollowTheLeader.Shared;

public class Player
{
    [JsonIgnore]
    public static List<string> Decorations => new()
    {
        "m -1 1 l -1 -1 l 2 1 l 2 -1 l -1 1 Z",
        "m -2 1 l 1 -1 l 1 1 l 1 -1 l 1 1 Z",
        "m 0 0 l -2 0 l 1 1 l 2 0 l 1 -1 Z",
        "m 0 0 l -2 0 l 1 1 l 1 -1 l 1 1 l 1 -1 Z"
    };
    [JsonIgnore]
    public static List<string> Colors => new()
    {
        "red", "green", "blue", "white"
    };
    public string Name { get; set; }
    public string ConnectionID { get; set; } = "";
    public string Color { get; set; } = "black";
    public double Heading { get; set; } = 45;
    public double Speed { get; set; } = 2;
    public string Decoration { get; set; } = Decorations[Random.Shared.Next(Decorations.Count())];
    public string DecorationFill { get; set; } = Colors[Random.Shared.Next(Colors.Count())];
    public double MoveSoundFrequenzy { get; set; }
    public double CollideSoundFrequenzy { get; set; }
    public Queue<Position> Body { get; set; } = new();
    public Position Head { get; set; } = new() { X = 50 + Random.Shared.NextDouble(), Y = 50 + Random.Shared.NextDouble() };
}
