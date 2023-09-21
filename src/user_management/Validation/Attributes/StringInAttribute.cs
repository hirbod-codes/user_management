using System.ComponentModel.DataAnnotations;

namespace user_management.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class StringInAttribute : ValidationAttribute
{
    public string[] Strings { get; private set; }

    public StringInAttribute(string[] strings) => Strings = strings;

    public override bool IsValid(object? value) => value != null && ((string)value) != null && Strings.Contains((string)value);
}
