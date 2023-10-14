using System.ComponentModel.DataAnnotations;
using user_management.Models;

namespace user_management.Validation.Attributes;

public class ReaderFields : ValidationAttribute
{
    public override bool IsValid(object? value) => ((Field[])value!).Where(f => User.GetReadableFields().FirstOrDefault<Field?>(ff => ff != null && ff.Name == f.Name) == null).Count() == 0;
}
