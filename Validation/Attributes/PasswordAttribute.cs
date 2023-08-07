using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace user_management.Validation.Attributes;

public class PasswordAttribute : ValidationAttribute
{
    public override string FormatErrorMessage(string name) => $"{name} must have at least 6 characters and must include one upper case, lower case, digit and special characters like (!@#$%^&*_.-).";

    public override bool IsValid(object? value) => ((string)value!).Length >= 6 && ((string)value!).Length <= 100 && DoesHaveUpperCase((string)value!) && DoesHaveLowerCase((string)value!) && DoesHaveNumber((string)value!) && DoesHaveSpecialCharacter((string)value!);

    private bool DoesHaveUpperCase(string str, int leastNumberOfUpperCaseLetters = 1) => str.Where(s => Char.IsUpper(s)).Count() >= leastNumberOfUpperCaseLetters;

    private bool DoesHaveLowerCase(string str, int leastNumberOfLowerCaseLetters = 1) => str.Where(s => Char.IsLower(s)).Count() >= leastNumberOfLowerCaseLetters;

    private bool DoesHaveNumber(string str, int leastNumberOfDigits = 1) => str.Where(s => Char.IsNumber(s)).Count() >= leastNumberOfDigits;

    private bool DoesHaveSpecialCharacter(string str, int leastNumberOfSpecialCharacters = 1) => Regex.Match(str, "[!@#$%^&*_.-]+").Success;
}
