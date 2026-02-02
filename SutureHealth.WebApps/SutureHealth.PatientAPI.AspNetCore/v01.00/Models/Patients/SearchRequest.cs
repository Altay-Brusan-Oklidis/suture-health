using System.ComponentModel.DataAnnotations;

namespace SutureHealth.Patients.v0100.Models.Patients;

public class SearchRequest
{
    public string Search { get; set; }
    public int? OrganizationId { get; set; }
    public int? Count { get; set; }
    public bool? IsCpoModal { get; set; } = false;
}
