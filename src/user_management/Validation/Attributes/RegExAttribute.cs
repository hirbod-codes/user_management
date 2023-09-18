using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace user_management.Validation.Attributes;

public class RegExAttribute : ValidationAttribute
{
    public string Pattern { get; set; } = null!;
    public RegExAttribute(string pattern, string? errorMessage = null)
    {
        Pattern = pattern;
        if (errorMessage != null) ErrorMessage = errorMessage;
    }

    public override string FormatErrorMessage(string name) => $"The {name} format is invalid.";
    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        if (value.GetType().Name != "String") return false;
        try { if (!Regex.IsMatch((value as string)!, Pattern)) return false; }
        catch (Exception) { return false; }
        return true;
    }
}
