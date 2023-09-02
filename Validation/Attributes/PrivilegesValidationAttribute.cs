using System.ComponentModel.DataAnnotations;
using user_management.Data;
using user_management.Models;

namespace user_management.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PrivilegesValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value != null && ((IEnumerable<Privilege>)value) == null && StaticData.AreValid((IEnumerable<Privilege>)value);
}