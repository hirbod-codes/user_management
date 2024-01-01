using System.ComponentModel.DataAnnotations;
using user_management.Data.Logics;
using user_management.Models;
using user_management.Utilities;

namespace user_management.Validation.Attributes;

public class MassUpdatable : ValidationAttribute
{
    public bool InvalidField { get; private set; } = false;
    public bool InvalidType { get; private set; } = false;
    public bool InvalidOperation { get; private set; } = false;

    public override string FormatErrorMessage(string name)
    {
        if (InvalidField)
            return $"Invalid update fields provided.";
        else if (InvalidType)
            return $"Invalid update types provided.";
        else if (InvalidOperation)
            return $"Invalid update operations provided.";
        else
            return $"Invalid updates provided.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not IEnumerable<Update> updates)
            return false;

        if (updates.Where(u => User.GetUpdatableFields().FirstOrDefault(f => f.Name == u.Field.ToSnakeCase()) is null).Any())
        {
            InvalidField = true;
            return false;
        }

        if (updates.Where(u => Types.AllTypes.FirstOrDefault(t => t == u.Type) is null).Any())
        {
            InvalidType = true;
            return false;
        }

        if (updates.Where(u => Update.AllOperations.FirstOrDefault(t => t == u.Operation) is null).Any())
        {
            InvalidOperation = true;
            return false;
        }

        return true;
    }
}
