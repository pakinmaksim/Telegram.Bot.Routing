using Newtonsoft.Json;

namespace Telegram.Bot.Routing.Storage.Serializers;

public class NewtonsoftJsonRouteDataSerializer : IRouteDataSerializer
{
    public object? Deserialization(string routeData, Type type)
    {
        return JsonConvert.DeserializeObject(routeData, type);
    }

    public string Serialization(object routeData)
    {
        return JsonConvert.SerializeObject(routeData);
    }
}