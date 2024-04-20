using System.Text.Json;

namespace GsmCore.Wrappers;

public static class JsonSerializerWrapper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static string SerializeCamelCase<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static T? DeserializeCamelCase<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options);
    }
}