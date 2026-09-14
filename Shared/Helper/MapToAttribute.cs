namespace Shared.Helper;

[AttributeUsage(AttributeTargets.Property)]
public class MapToAttribute : Attribute
{
    public string TargetName { get; }
    public MapToAttribute(string targetName) => TargetName = targetName;
}
