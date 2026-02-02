using Newtonsoft.Json;

namespace SutureHealth.Hchb;
#nullable enable
public class HchbPatientWeb 
{
    public int Id { get; set; }
        
    /// <summary>
    /// Suture Health PatientId
    /// </summary>
    public int? PatientId { get; set; }
  
    public string? HchbPatientId { get; set; }
        
    public string? EpisodeId { get; set; }
    
    public string? Status { get; set; }

    public string? IcdCode { get; set; }        
}