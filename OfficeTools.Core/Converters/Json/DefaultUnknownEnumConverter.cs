using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OfficeTools.Core.Converters.Json;

public class DefaultUnknownEnumConverter<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]
    T
> : JsonConverter<T>
    where T : Enum
{
    /// <summary>
    ///     Lazy initialization for <see cref="EnumMemberNames" />.
    /// </summary>
    readonly private Lazy<Dictionary<T, string>> _enumMemberNamesLazy;

    /// <summary>
    ///     Lazy initialization for <see cref="EnumMemberValues" />.
    /// </summary>
    readonly private Lazy<Dictionary<string, T>> _enumMemberValuesLazy =
        new(
            () =>
                typeof(T)
                    .GetFields()
                    .Where(field => field.IsStatic)
                    .Select(
                        field =>
                            new
                            {
                                FieldName = field.Name,
                                FieldValue = (T)field.GetValue(null)!,
                                EnumMemberValue = field
                                    .GetCustomAttributes<EnumMemberAttribute>(false)
                                    .FirstOrDefault()
                                    ?.Value?.ToString()
                            }
                    )
                    .ToDictionary(x => x.EnumMemberValue ?? x.FieldName, x => x.FieldValue)
        );

    public DefaultUnknownEnumConverter()
    {
        _enumMemberNamesLazy = new Lazy<Dictionary<T, string>>(
            () => EnumMemberValues.ToDictionary(x => x.Value, x => x.Key)
        );
    }

    /// <summary>
    ///     Gets a dictionary of enum member values, keyed by the EnumMember attribute value, or the field name if no
    ///     EnumMember attribute is present.
    /// </summary>
    private Dictionary<string, T> EnumMemberValues => _enumMemberValuesLazy.Value;

    /// <summary>
    ///     Gets a dictionary of enum member names, keyed by the enum member value.
    /// </summary>
    private Dictionary<T, string> EnumMemberNames => _enumMemberNamesLazy.Value;

    /// <summary>
    ///     Gets the value of the "Unknown" enum member, or the 0 value if no "Unknown" member is present.
    /// </summary>
    private T UnknownValue => EnumMemberValues.TryGetValue("Unknown", out T? res) ? res : (T)Enum.ToObject(typeof(T), 0);

    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
        {
            throw new JsonException("Expected String or PropertyName token");
        }

        if (reader.GetString() is { } readerString)
        {
            // First try get exact match
            if (EnumMemberValues.TryGetValue(readerString, out T? enumMemberValue))
            {
                return enumMemberValue;
            }

            // Otherwise try get case-insensitive match
            if (
                EnumMemberValues.Keys.FirstOrDefault(
                    key => key.Equals(readerString, StringComparison.OrdinalIgnoreCase)
                ) is
                { } enumMemberName
            )
            {
                return EnumMemberValues[enumMemberName];
            }

            Debug.WriteLine($"Unknown enum member value for {typeToConvert}: {readerString}");
        }

        return UnknownValue;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(EnumMemberNames[value]);
    }

    /// <inheritdoc />
    public override T ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        Read(ref reader, typeToConvert, options);

    /// <inheritdoc />
    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        T? value,
        JsonSerializerOptions options
    ) =>
        Write(writer, value, options);
}