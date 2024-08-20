using System.Text.Json.Serialization;

namespace OfficeTools.Core.Models.Settings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HolidayMode
{
    Automatic,
    Enabled,
    Disabled
}