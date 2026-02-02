using System;
using System.Collections.Generic;

namespace SutureHealth.Application;

public class FeatureFlag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Active { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeleteDate { get; set; }
    public DateTime? RestoreDate { get; set; }
    public bool HasCohort { get; set; }   

    // Navigation property for FeatureFlags_Users
    public ICollection<FeatureFlagsUsers> FeatureFlagsUsers { get; set; }
}
