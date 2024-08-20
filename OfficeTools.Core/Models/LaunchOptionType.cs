using System.Text.Json.Serialization;

namespace OfficeTools.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter<LaunchOptionType>))]
public enum LaunchOptionType
{
    Bool,
    String,
    Int
}