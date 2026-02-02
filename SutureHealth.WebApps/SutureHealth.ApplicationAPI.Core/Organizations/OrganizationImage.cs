using System;

namespace SutureHealth.Application;

public class OrganizationImage
{
    public Guid OrganizationImageId { get; set; }
    public int OrganizationId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadDate { get; set; }
    public bool Active { get; set; }
}

