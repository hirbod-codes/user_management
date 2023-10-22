using System.ComponentModel.DataAnnotations;
using user_management.Models;

namespace user_management.Validation.Attributes;

public class ReaderFields : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        List<Field> readableFields = User.GetReadableFields();

        return (value as IEnumerable<Field>) != null && !(value as IEnumerable<Field>)!.Where(f => readableFields.FirstOrDefault(ff => ff != null && ff.Name == f.Name) == null).Any();
    }
}
