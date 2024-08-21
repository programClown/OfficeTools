using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OfficeTools.Core.Validators;

/// <summary>
///     Validator that requires equality to another property
///     i.e. Confirm password must match password
/// </summary>
public sealed class RequiresMatchAttribute<T> : ValidationAttribute
    where T : IEquatable<T>
{
    public RequiresMatchAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    public RequiresMatchAttribute(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    public string PropertyName { get; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;

        PropertyInfo otherProperty =
            instance.GetType().GetProperty(PropertyName)
            ?? throw new ArgumentException($"Property {PropertyName} not found");

        if (otherProperty.PropertyType != typeof(T))
        {
            throw new ArgumentException($"Property {PropertyName} is not of type {typeof(T)}");
        }

        var otherValue = otherProperty.GetValue(instance);

        if (otherValue == null && value == null)
        {
            return ValidationResult.Success!;
        }

        if (((IEquatable<T>?)otherValue)!.Equals(value))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            $"{validationContext.DisplayName} does not match {PropertyName}"
        );
    }
}