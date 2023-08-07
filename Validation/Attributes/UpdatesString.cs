using System.ComponentModel.DataAnnotations;

namespace user_management.Validation.Attributes;

public class UpdatesString : ValidationAttribute
{
    public override string FormatErrorMessage(string name) => $"{name} is invalid.";
    public override bool IsValid(object? value) => true;
}
