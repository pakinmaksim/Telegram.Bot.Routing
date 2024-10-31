using System.Text.RegularExpressions;

namespace Telegram.Bot.Routing.Contexts.Messages;

/// <summary>
/// Represents a callback query data helper for Telegram.Bot, allowing easy construction, parsing, and management of callback queries.
/// </summary>
public partial class CallbackData
{
    /// <summary>
    /// Regular expression to parse the root structure of the callback data.
    /// </summary>
    [GeneratedRegex(@"(?<action>[^\?]*)(?:\?(?<values>.+))?")]
    private static partial Regex RootRegex();
    
    /// <summary>
    /// Regular expression to parse key-value pairs in the callback data.
    /// </summary>
    [GeneratedRegex(@"(?<key>[^\=]+)=(?<value>.+)")]
    private static partial Regex PropertyRegex();
    
    /// <summary>
    /// Creates a new instance of <see cref="CallbackData"/> with the specified action.
    /// </summary>
    /// <param name="action">The action part of the callback data.</param>
    /// <returns>A new <see cref="CallbackData"/> instance.</returns>
    public static CallbackData Create(string? action)
    {
        return new CallbackData(action, new Dictionary<string, string>());
    }
    
    /// <summary>
    /// Parses a callback data string into a <see cref="CallbackData"/> object.
    /// </summary>
    /// <param name="value">The callback data string to parse.</param>
    /// <returns>A <see cref="CallbackData"/> instance with parsed action and key-value pairs.</returns>
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
    
    /// <summary>
    /// Gets the action part of the callback data.
    /// </summary>
    public string? Action { get; private set; }
    /// <summary>
    /// Dictionary to store key-value pairs associated with the callback data.
    /// </summary>
    private readonly Dictionary<string, string> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallbackData"/> class.
    /// </summary>
    /// <param name="action">The action part of the callback data.</param>
    /// <param name="values">A dictionary containing key-value pairs.</param>
    private CallbackData(string? action, Dictionary<string, string> values)
    {
        Action = action;
        _values = values;
    }
    
    /// <summary>
    /// Sets or updates a string value in the callback data.
    /// </summary>
    /// <param name="key">The key to set or update.</param>
    /// <param name="value">The value to set. If null or empty, the key is removed.</param>
    /// <returns>The current <see cref="CallbackData"/> instance.</returns>
    public CallbackData SetValue(string key, string? value)
    {
        if (string.IsNullOrEmpty(value)) _values.Remove(key);
        else if (!_values.TryAdd(key, value)) _values[key] = value;
        return this;
    }
    
    /// <summary>
    /// Sets or updates an integer value in the callback data.
    /// </summary>
    /// <param name="key">The key to set or update.</param>
    /// <param name="value">The integer value to set.</param>
    /// <returns>The current <see cref="CallbackData"/> instance.</returns>
    public CallbackData SetValue(string key, int? value)
    {
        return SetValue(key, value?.ToString());
    }
    
    /// <summary>
    /// Sets or updates a long value in the callback data.
    /// </summary>
    /// <param name="key">The key to set or update.</param>
    /// <param name="value">The long value to set.</param>
    /// <returns>The current <see cref="CallbackData"/> instance.</returns>
    public CallbackData SetValue(string key, long? value)
    {
        return SetValue(key, value?.ToString());
    }

    /// <summary>
    /// Sets or updates a GUID value in the callback data.
    /// </summary>
    /// <param name="key">The key to set or update.</param>
    /// <param name="value">The GUID value to set, formatted as a string.</param>
    /// <returns>The current <see cref="CallbackData"/> instance.</returns>
    public CallbackData SetValue(string key, Guid? value)
    {
        return SetValue(key, value?.ToString("N"));
    }

    /// <summary>
    /// Sets or updates a DateTime value in the callback data, storing it as ticks.
    /// </summary>
    /// <param name="key">The key to set or update.</param>
    /// <param name="value">The DateTime value to set.</param>
    /// <returns>The current <see cref="CallbackData"/> instance.</returns>
    public CallbackData SetValue(string key, DateTime? value)
    {
        return SetValue(key, value?.Ticks);
    }
    
    /// <summary>
    /// Sets or updates a DateTimeOffset value in the callback data, storing ticks and offset.
    /// </summary>
    /// <param name="key">The key to set or update.</param>
    /// <param name="value">The DateTimeOffset value to set.</param>
    /// <returns>The current <see cref="CallbackData"/> instance.</returns>
    public CallbackData SetValue(string key, DateTimeOffset? value)
    {
        return SetValue(key, value is null ? null : $"{value.Value.Ticks}+{value.Value.Offset.Ticks}");
    }

    /// <summary>
    /// Attempts to retrieve a string value from the callback data.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="value">The retrieved string value.</param>
    /// <returns>True if the key was found and the value is non-empty, otherwise false.</returns>
    public bool TryGetString(string key, out string value)
    {
        return _values.TryGetValue(key, out value!) && !string.IsNullOrEmpty(value);
    }
    
    /// <summary>
    /// Attempts to retrieve an integer value from the callback data.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="value">The retrieved integer value.</param>
    /// <returns>True if the key was found and the value was successfully parsed, otherwise false.</returns>
    public bool TryGetInt(string key, out int value)
    {
        value = default;
        return TryGetString(key, out var stringValue) &&
               int.TryParse(stringValue, out value);
    }
    
    /// <summary>
    /// Attempts to retrieve a long value from the callback data.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="value">The retrieved long value.</param>
    /// <returns>True if the key was found and the value was successfully parsed, otherwise false.</returns>
    public bool TryGetLong(string key, out long value)
    {
        value = default;
        return TryGetString(key, out var stringValue) &&
               long.TryParse(stringValue, out value);
    }
    
    /// <summary>
    /// Attempts to retrieve a GUID value from the callback data.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="value">The retrieved GUID value.</param>
    /// <returns>True if the key was found and the value was successfully parsed, otherwise false.</returns>
    public bool TryGetGuid(string key, out Guid value)
    {
        value = default;
        return TryGetString(key, out var stringValue) &&
               Guid.TryParseExact(stringValue, "N", out value);
    }
    
    /// <summary>
    /// Attempts to retrieve a DateTime value from the callback data.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="value">The retrieved DateTime value.</param>
    /// <returns>True if the key was found and the value was successfully parsed, otherwise false.</returns>
    public bool TryGetDateTime(string key, out DateTime value)
    {
        value = default;
        if (!TryGetLong(key, out var longValue)) return false;
        value = new DateTime(longValue);
        return true;
    }
    
    /// <summary>
    /// Attempts to retrieve a DateTimeOffset value from the callback data.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="value">The retrieved DateTimeOffset value.</param>
    /// <returns>True if the key was found and the value was successfully parsed, otherwise false.</returns>
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

    /// <summary>
    /// Converts the callback data to its string representation.
    /// </summary>
    /// <returns>The string representation of the callback data, including action and key-value pairs.</returns>
    public override string ToString()
    {
        var valuesString = string.Join("&", _values.Select(x => $"{x.Key}={x.Value}"));
        return $"{Action}" + (valuesString.Length > 0 ? "?" + valuesString : string.Empty);
    }
}