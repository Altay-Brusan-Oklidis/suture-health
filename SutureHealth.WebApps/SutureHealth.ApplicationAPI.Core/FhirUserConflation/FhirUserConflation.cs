using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SutureHealth.Application;

public class FhirUserConflation : IValidatableObject
{
    [Required] [Key] public string FhirId { get; set; }
    [Required] public int UserId { get; set; }

    private ValidationResult FhirIdValidation => !FhirId.IsNullOrWhiteSpace() && FhirId.Length <= 50
        ? ValidationResult.Success
        : new ValidationResult($"FhirId = {FhirId} is invalid.");

    private ValidationResult UserIdValidation =>
        UserId >= 0 ? ValidationResult.Success : new ValidationResult($"UserId = {UserId} is invalid.");

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield return FhirIdValidation;
        yield return UserIdValidation;
    }

    public bool IsValid()
    {
        return Validate(new ValidationContext(this)).All((result) => result == ValidationResult.Success);
    }
}