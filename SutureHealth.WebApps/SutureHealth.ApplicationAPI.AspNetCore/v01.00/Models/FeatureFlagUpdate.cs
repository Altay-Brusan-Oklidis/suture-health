namespace SutureHealth.Application.v0100.Models;

public class FeatureFlagUpdate
{
    public int Id { get; set; }
    public string ProductArea { get; set; }
    public string FeatureName { get; set; }
    public string SubfeatureName { get; set; }
    public string Description { get; set; }
}
