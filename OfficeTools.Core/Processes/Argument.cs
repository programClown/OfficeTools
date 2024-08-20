using OneOf;

namespace OfficeTools.Core.Processes;

[GenerateOneOf]
public partial class Argument : OneOfBase<string, (string, string)>
{
}