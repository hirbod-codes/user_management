using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace user_management.Validation.Attributes;

public class ObjectIdAttribute : ValidationAttribute
{
    public override string FormatErrorMessage(string name) => $"{name} is not a valid id.";
    public override bool IsValid(object? value) => ObjectId.TryParse((string)value!, out ObjectId id);
}
