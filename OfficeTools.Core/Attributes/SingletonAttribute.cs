using System.Diagnostics.CodeAnalysis;

namespace OfficeTools.Core.Attributes;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class SingletonAttribute : Attribute
{
    public SingletonAttribute()
    {
    }

    public SingletonAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public SingletonAttribute(Type interfaceType, Type implType)
    {
        InterfaceType = implType;
        ImplType = implType;
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? InterfaceType { get; init; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? ImplType { get; init; }
}