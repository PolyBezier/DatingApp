namespace API.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AutoRegisterAttribute : Attribute
{
    public bool AsInterface { get; init; } = true;
    public Type? CustomInterface { get; init; }
}
