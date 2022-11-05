using System.Globalization;

namespace FollowTheLeader.Client.Extensions;

public static class DecimalExtensions
{
    public static string AsString(this decimal d) => d.ToString(CultureInfo.InvariantCulture);
}
