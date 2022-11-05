using System.Globalization;

namespace FollowTheLeader.Client.Extensions;

public static class DoubleExtensions
{
    public static string AsString(this double d) => d.ToString(CultureInfo.InvariantCulture);
}
