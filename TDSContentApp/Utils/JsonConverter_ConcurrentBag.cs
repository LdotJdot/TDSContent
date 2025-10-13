using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ConcurrentBagConverter<T> : JsonConverter<ConcurrentBag<T>>
{
    public override ConcurrentBag<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 反序列化为 List<T>
        var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
        if (list == null)
        {
            return null;
        }

        // 将 List<T> 转换为 ConcurrentBag<T>
        return [.. list];
    }

    public override void Write(Utf8JsonWriter writer, ConcurrentBag<T> value, JsonSerializerOptions options)
    {
        // 将 ConcurrentBag<T> 转换为 List<T> 并序列化
        var list = new List<T>(value);
        JsonSerializer.Serialize(writer, list, options);
    }
}