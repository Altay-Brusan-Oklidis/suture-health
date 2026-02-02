using System;

namespace SutureHealth.Documents
{
    public class FaceToFaceAttributes
    {
        public string RequestId { get; set; }
        public string Signature { get; set; }
        public string Npi { get; set; }
        public string SendingOrganizationName { get; set; }
        public string Patient { get; set; }
        public string EpisodeEffectiveDate { get; set; }
        public string EncounterDate { get; set; }
        public string MedicalCondition { get; set; }
        public bool NursingRequired { get; set; }
        public bool PhysicialTherapyRequired { get; set; }
        public bool SpeechTherapyRequired { get; set; }
        public bool OccupationalTherapyRequired { get; set; }
        public string ClinicalReasonForHomeCare { get; set; }
        public string ReasonForBeingHomebound { get; set; }
        public DateTimeOffset DateSigned { get; set; }
        public string SignatureId { get; set; }
        public string Pid { get; set; }
        public string TreatmentPlan { get; set; }   // Optional
    }
}
