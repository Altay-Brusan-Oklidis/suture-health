using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SutureHealth.Hchb;

public class HchbTransaction
{
    public int Id { get; set; }

    /// <summary>
    /// Suture Request ID
    /// </summary>
    public int RequestId { get; set; }
    public DateTime? OrderDate { get; set; } = null;
    public string OrderNumber { get; set; }
    public string FileName { get; set; }
    public DateTime? AdmitDate { get; set; } = null;
    public string AdmissionType { get; set; }
    public string ObservationId { get; set; }
    public string ObservationText { get; set; }
    public string PatientType { get; set; }

    /// <summary>
    /// Suture Health Template ID
    /// </summary>
    public int TemplateId { get; set; }
    public int SignerId { get; set; }
    public int SignerFacilityId { get; set; }
    public int? Status { get; set; }
    public DateTime? SendDate { get; set; } = null;
    public string HchbPatientId { get ; set; }
    public string EpisodeId {  get; set; }
}