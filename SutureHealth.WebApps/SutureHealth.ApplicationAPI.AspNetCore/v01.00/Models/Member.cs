using System;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.Application.v0100.Models;

public class Member
{
    public int MemberId { get; set; }
    public string NPI { get; set; }
    public int MemberTypeId { get; set; }
    public string SigningName { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string MaidenName { get; set; }
    public string Suffix { get; set; }
    public string ProfessionalSuffix { get; set; }

    public virtual string Email { get; set; }
    public virtual string MobileNumber { get; set; }

    public bool CanSign { get; set; } = false;
    public bool IsActive { get; set; }
    public bool IsCollaborator { get; set; }
    public bool IsPayingClient { get; set; }
    public DateTimeOffset? LastLoggedInAt { get; set; }
    public bool HasSingleSigner { get; set; } = false;
    public bool HasSingleCollaborator { get; set; } = false;
}