namespace Reusable.Apps.Server.Json;

public class JsonString
{
    private readonly string _json;

    public JsonString(string? json) => _json = json;

    public override string ToString() => _json;

    public static implicit operator string(JsonString jsonString) => jsonString._json;
}