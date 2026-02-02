using System;

namespace SutureHealth.Patients.v0100.Models;

public class PatientListItem
{
    public int PatientId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Suffix { get; set; }
    public DateTime Birthdate { get; set; }
    public int TotalCpoTimeThisMonth { get; set; } = -1;
}
