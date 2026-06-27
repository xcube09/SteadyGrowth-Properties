using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SteadyGrowth.Web.Application.Validation;

public class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object? value)
    {
        return value is bool boolValue && boolValue;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-mustbetrue", ErrorMessage ?? "This field must be checked.");
    }
}
