using System.ComponentModel.DataAnnotations;
using user_management.Models;

namespace user_management.Validation.Attributes;

public class UpdaterFields : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        List<Field> updatableFields = User.GetUpdatableFields();

        return (value as IEnumerable<Field>) != null && !(value as IEnumerable<Field>)!.Where(f => updatableFields.FirstOrDefault<Field?>(ff => ff != null && ff.Name == f.Name) == null).Any();
    }
}
