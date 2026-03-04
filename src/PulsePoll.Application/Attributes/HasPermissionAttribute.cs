namespace PulsePoll.Application.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class HasPermissionAttribute(params string[] permissions) : Attribute
{
    public string[] Permissions { get; } = permissions;
}
