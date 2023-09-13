using System.ComponentModel.DataAnnotations;
using user_management.Models;

namespace user_management.Validation.Attributes;

public class DeleterAuthor : ValidationAttribute
{
    public override string FormatErrorMessage(string name) => $"{name} is not a valid author.";
    public override bool IsValid(object? value) => Deleter.ValidateAuthor((string)value!);
}
