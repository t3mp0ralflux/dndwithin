using System.Text.Json;

namespace Testing.Common;

public static class Converters
{
    public static T Clone<T>(this T item)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item))!;
    }

    public static string RandomizeCasing(this string input)
    {
        Random random = new();

        IEnumerable<char> transformed = input.Select(x => random.Next() % 2 == 0
                                                         ? char.IsUpper(x)
                                                             ? x.ToString().ToLower().First()
                                                             : x.ToString().ToUpper().First()
                                                         : x);

        string result = new(transformed.ToArray());

        return result;
    }
}