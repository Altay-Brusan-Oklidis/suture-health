using MessageSenderAgent.Model.Visit;
using MessageSenderAgent.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Builder
{
    public class PV1SegmentBuilder
    {
        VisitModel visitModel;
        AssignedPatientLocation? location;
        AttendingDoctorType? attendingDoctor = null;
        ReferringAndConsultingDoctorType? referringDoctor = null;
        ReferringAndConsultingDoctorType? consultingDoctor = null;
        PatientType? patientType = null;
        string? visitNumber = null;
        string? dischargeDisposition = null;

        public PV1SegmentBuilder( string setId, 
                                  string? addmissionType = null, 
                                  AssignedPatientLocation? location = null,
                                  AttendingDoctorType? attendingDoctor = null,
                                  ReferringAndConsultingDoctorType? referringDoctor=null,
                                  ReferringAndConsultingDoctorType? consultingDoctor = null,
                                  PatientType? patientType = null,
                                  string? visitNumber = null,
                                  string? dischargeDisposition = null) 
        {
            visitModel = new VisitModel();
            visitModel.SetId = setId;
            this.location = location;
            if (addmissionType != null) 
            {
                this.visitModel.AdmissionType = addmissionType;

            }
            else
            {
                Random rnd = new Random();
                var propertiesList = typeof(AdmissionType).GetProperties();
                var randomAdmissionType = (string?)propertiesList[rnd.Next(0, propertiesList.Count())].GetValue(null, null);
                this.visitModel.AdmissionType = randomAdmissionType;
            }

            this.attendingDoctor = attendingDoctor;
            this.referringDoctor = referringDoctor;
            this.consultingDoctor = consultingDoctor;
            this.patientType = patientType;
            this.visitNumber = visitNumber;
            this.dischargeDisposition = dischargeDisposition;
        }

        public VisitModel Build() 
        {            
            
            if (location == null) 
            {
                location = new AssignedPatientLocation();
                location.AgencyName = Utilities.GetRandomAlphabeticString(4);
                location.BranchCode = Utilities.GetRandomalphabeticString(2);
                location.BranchName = Utilities.GetRandomalphabeticString(5);
                location.TeamName = Utilities.GetRandomAlphabeticString(5);
                location.ServiceLine = Utilities.GetRandomProperyValue<ServiceLocationType>();
            }
            visitModel.AssignedPatientLocation = location;

            if (attendingDoctor == null) 
            {
                attendingDoctor = new AttendingDoctorType()
                {
                    FirstName = Utilities.GetRandomNameOrFamilyName("FirstName"),
                    LastName = Utilities.GetRandomNameOrFamilyName("LastName"),
                    NPI = Utilities.GetRandomAlphabeticString(5)
                };

            }
            else             
            {
                this.visitModel.AttendingDoctor = attendingDoctor;
            }

            if (referringDoctor == null)
            {
                referringDoctor = new ReferringAndConsultingDoctorType()
                {

                    FirstName = Utilities.GetRandomNameOrFamilyName("Name"),
                    LastName = Utilities.GetRandomNameOrFamilyName("LastName"),
                    NPI = Utilities.GetRandomAlphabeticString(5),
                    Degree = DegreeLicenseCertificateType.MD
                };

            }
            else
            {
                this.visitModel.ReferringDoctor = referringDoctor;
            }

            if (consultingDoctor == null)
            {
                consultingDoctor = new ReferringAndConsultingDoctorType()
                {

                    FirstName = Utilities.GetRandomNameOrFamilyName("Name"),
                    LastName = Utilities.GetRandomNameOrFamilyName("LastName"),
                    NPI = Utilities.GetRandomAlphabeticString(5),
                    Degree = DegreeLicenseCertificateType.MD
                };
            }
            else
            {
                this.visitModel.ConsultingDoctor = consultingDoctor;
            }

            if(patientType == null) 
            {
                this.visitModel.PatientType = patientType?.HOMEHEALTH;
            }
            else 
            {
                Random rnd = new Random();
                var propertiesList = typeof(PatientType).GetProperties();
                var patientType = (string?)propertiesList[rnd.Next(0, propertiesList.Count())].GetValue(null, null);
                this.visitModel.PatientType = patientType;
            }
            visitModel.VisitNumber = visitNumber ?? Utilities.GetRandomDecimalString(3);
            visitModel.DischargeDisposition = dischargeDisposition ?? Utilities.GetRandomDecimalString(3);

            return visitModel;
        }
    }
}
