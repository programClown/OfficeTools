using System.Text.Json.Serialization;

namespace OfficeTools.Core.Python;

public record QueryTclTkLibraryResult(string? TclLibrary, string? TkLibrary);

[JsonSerializable(typeof(QueryTclTkLibraryResult))]
internal partial class QueryTclTkLibraryResultJsonContext : JsonSerializerContext;