using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using OfficeTools.Core.Processes;

namespace OfficeTools.Core.Python;

public partial record PipPackageSpecifier
{
    public required string Name { get; init; }

    public string? Constraint { get; init; }

    public string? Version { get; init; }

    public string? VersionConstraint => Constraint is null || Version is null ? null : Constraint + Name;

    public static PipPackageSpecifier Parse(string value)
    {
        var result = TryParse(value, true, out PipPackageSpecifier? packageSpecifier);

        Debug.Assert(result);

        return packageSpecifier!;
    }

    public static bool TryParse(string value, [NotNullWhen(true)] out PipPackageSpecifier? packageSpecifier) =>
        TryParse(value, false, out packageSpecifier);

    private static bool TryParse(
        string value,
        bool throwOnFailure,
        [NotNullWhen(true)] out PipPackageSpecifier? packageSpecifier
    )
    {
        Match match = PackageSpecifierRegex().Match(value);
        if (!match.Success)
        {
            if (throwOnFailure)
            {
                throw new ArgumentException($"Invalid package specifier: {value}");
            }

            packageSpecifier = null;
            return false;
        }

        packageSpecifier = new PipPackageSpecifier
        {
            Name = match.Groups["package_name"].Value,
            Constraint = match.Groups["version_constraint"].Value,
            Version = match.Groups["version"].Value
        };

        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Name + VersionConstraint;

    public static implicit operator Argument(PipPackageSpecifier specifier) =>
        specifier.VersionConstraint is null
            ? new Argument(specifier.Name)
            : new Argument((specifier.Name, specifier.VersionConstraint));

    public static implicit operator PipPackageSpecifier(string specifier) => Parse(specifier);

    /// <summary>
    ///     Regex to match a pip package specifier.
    /// </summary>
    [GeneratedRegex(
        "(?<package_name>[a-zA-Z0-9_]+)(?<version_specifier>(?<version_constraint>==|>=|<=|>|<|~=|!=)(<version>[a-zA-Z0-9_.]+))?",
        RegexOptions.CultureInvariant,
        1000
    )]
    private static partial Regex PackageSpecifierRegex();
}