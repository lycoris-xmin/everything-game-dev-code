using System.Text.Json;

namespace XianxiaRPG.Systems.Persistence;

/// <summary>SYS-005 存档（最小契约）：JSON 序列化/反序列化，可回放恢复。</summary>
public static class SaveSystem
{
    public static string ToJson(SaveData data)
        => JsonSerializer.Serialize(data, new JsonSerializerOptions { IncludeFields = true });

    public static SaveData FromJson(string json)
        => JsonSerializer.Deserialize<SaveData>(json, new JsonSerializerOptions { IncludeFields = true })!;
}
