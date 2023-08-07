using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace user_management.Validation.Attributes;

public class RegExAttribute : RegularExpressionAttribute
{
    public RegExAttribute([StringSyntax("Regex")] string pattern, string? errorMessage = null) : base(pattern)
    {
        if (errorMessage != null) ErrorMessage = errorMessage;
    }
}
