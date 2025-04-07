using System.Text.Json;

namespace Testing.Common;

public static class Converters
{
    public static T Clone<T>(this T item)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item))!;
    }
}