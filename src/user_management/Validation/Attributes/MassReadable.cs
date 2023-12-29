using System.ComponentModel.DataAnnotations;
using user_management.Data.Logics;
using user_management.Models;

namespace user_management.Validation.Attributes;

public class MassReadable : ValidationAttribute
{
    public bool InvalidField { get; private set; } = false;
    public bool InvalidType { get; private set; } = false;
    public bool InvalidOperation { get; private set; } = false;

    public override string FormatErrorMessage(string name)
    {
        if (InvalidField)
            return $"Invalid filter fields provided.";
        else if (InvalidType)
            return $"Invalid filter types provided.";
        else if (InvalidOperation)
            return $"Invalid filter operations provided.";
        else
            return $"Invalid filters provided.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not Filter filter)
            return false;

        if (filter.GetFields().Where(f => User.GetUpdatableFields().FirstOrDefault(ff => ff.Name == f) is null).Any())
        {
            InvalidField = true;
            return false;
        }

        if (filter.GetTypes().Where(f => Types.AllTypes.FirstOrDefault(t => t == f) is null).Any())
        {
            InvalidType = true;
            return false;
        }

        if (filter.GetOperations().Where(f => Filter.AllOperations.FirstOrDefault(o => o == f) is null).Any())
        {
            InvalidOperation = true;
            return false;
        }

        return true;
    }
}
