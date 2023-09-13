using System.ComponentModel.DataAnnotations;

namespace user_management.Validation.Attributes;

public class PasswordConfirmationAttribute<T> : ValidationAttribute where T : IPasswordConfirmable
{
    public override string FormatErrorMessage(string name) => $"Password and Password Confirmation don't match.";

    public override bool IsValid(object? value) => ((T)value!).Password == ((T)value!).PasswordConfirmation;
}
