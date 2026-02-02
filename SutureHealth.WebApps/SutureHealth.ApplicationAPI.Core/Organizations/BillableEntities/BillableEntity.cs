namespace SutureHealth.Application;

public class BillableEntity
{
    public int BillableEntityId { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationType { get; set; }
    public int SystemServiceId { get; set; }
    public bool IsSubscribed { get; set; }
}