namespace Telegram.Bot.Routing.Storage.Serializers;

public interface IRouteDataSerializer
{
    public object? Deserialization(string routeData, Type type);
    public object? Deserialize(string routeData, Type type) => 
        type == typeof(string) ? routeData : Deserialization(routeData, type);
    public object? DeserializeNullable(string? routeData, Type type) => 
        string.IsNullOrEmpty(routeData) ? null : Deserialize(routeData, type);

    public string Serialization(object routeData);
    public string Serialize(object routeData) => 
        routeData as string ?? Serialization(routeData);
    public string? SerializeNullable(object? routeData) => 
        routeData is null ? null : Serialize(routeData);
}