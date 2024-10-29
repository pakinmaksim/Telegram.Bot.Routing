using System.Text.RegularExpressions;

namespace Telegram.Bot.Routing.Contexts.Messages;

public partial class CallbackData
{
    [GeneratedRegex(@"(?<action>[^\?]*)(?:\?(?<values>.+))?")]
    private static partial Regex RootRegex();
    [GeneratedRegex(@"(?<key>[^\=]+)=(?<value>.+)")]
    private static partial Regex PropertyRegex();
    
    public static CallbackData Create(string? action)
    {
        return new CallbackData(action, new Dictionary<string, string>());
    }
    
    public static CallbackData Parse(string? value)
    {
        if (value is null) return new CallbackData(null, new Dictionary<string, string>());

        var match = RootRegex().Match(value);
        match.Groups.TryGetValue("action", out var actionGroup);
        match.Groups.TryGetValue("values", out var valuesGroup);

        var action = string.IsNullOrEmpty(actionGroup?.Value) ? null : actionGroup.Value;
        var valuesString = valuesGroup?.Value ?? "";

        var values = valuesString.Split("&", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => PropertyRegex().Match(x))
            .Where(x => x.Success)
            .ToDictionary(x => x.Groups["key"].Value, x => x.Groups["value"].Value);
        
        return new CallbackData(action, values);
    }
    
    public string? Action { get; private set; }
    private readonly Dictionary<string, string> _values;

    private CallbackData(string? action, Dictionary<string, string> values)
    {
        Action = action;
        _values = values;
    }

    public CallbackData SetValue(string key, string? value)
    {
        if (string.IsNullOrEmpty(value)) _values.Remove(key);
        else if (!_values.TryAdd(key, value)) _values[key] = value;
        return this;
    }
    public CallbackData SetValue(string key, int? value)
    {
        return SetValue(key, value?.ToString());
    }
    public CallbackData SetValue(string key, long? value)
    {
        return SetValue(key, value?.ToString());
    }
    public CallbackData SetValue(string key, Guid? value)
    {
        return SetValue(key, value?.ToString("N"));
    }
    public CallbackData SetValue(string key, DateTime? value)
    {
        return SetValue(key, value?.Ticks);
    }
    public CallbackData SetValue(string key, DateTimeOffset? value)
    {
        return SetValue(key, value is null ? null : $"{value.Value.Ticks}+{value.Value.Offset.Ticks}");
    }

    public bool TryGetString(string key, out string value)
    {
        return _values.TryGetValue(key, out value!) && !string.IsNullOrEmpty(value);
    }
    public bool TryGetInt(string key, out int value)
    {
        value = default;
        return TryGetString(key, out var stringValue) &&
               int.TryParse(stringValue, out value);
    }
    public bool TryGetLong(string key, out long value)
    {
        value = default;
        return TryGetString(key, out var stringValue) &&
               long.TryParse(stringValue, out value);
    }
    public bool TryGetGuid(string key, out Guid value)
    {
        value = default;
        return TryGetString(key, out var stringValue) &&
               Guid.TryParseExact(stringValue, "N", out value);
    }
    public bool TryGetDateTime(string key, out DateTime value)
    {
        value = default;
        if (!TryGetLong(key, out var longValue)) return false;
        value = new DateTime(longValue);
        return true;
    }
    public bool TryGetDateTimeOffset(string key, out DateTimeOffset value)
    {
        value = default;
        if (!_values.TryGetValue(key, out var stringValue)) return false;
        
        var ticks = stringValue.Split("+", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (ticks.Length < 2) return false;

        if (!long.TryParse(ticks[0], out var dateTicks) || !long.TryParse(ticks[1], out var timeZoneTicks))
            return false;
        
        value = new DateTimeOffset(dateTicks, new TimeSpan(timeZoneTicks));
        return true;
    }

    public override string ToString()
    {
        var valuesString = string.Join("&", _values.Select(x => $"{x.Key}={x.Value}"));
        return $"{Action}" + (valuesString.Length > 0 ? "?" + valuesString : string.Empty);
    }
}