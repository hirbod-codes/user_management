using System.ComponentModel.DataAnnotations;

namespace user_management.Validation.Attributes;

public class FiltersString : ValidationAttribute
{
    public override string FormatErrorMessage(string name) => $"{name} is invalid.";
    public override bool IsValid(object? value) => true;
}
