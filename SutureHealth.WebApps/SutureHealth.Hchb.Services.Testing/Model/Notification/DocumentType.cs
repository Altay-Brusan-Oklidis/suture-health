using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.PatientAPI.Services.Testing.Model.Notification
{
    /// <summary>
    /// HL7 Table - 0270 - Document Type
    /// <warning>
    /// HCHB default value is PDF. This item is not available in HL7 
    /// default standard and it is included manually. 
    /// </warning>
    /// </summary>
    public enum DocumentType
    {
        AR,  //  Autopsy report
        CD,  //  Cardiodiagnostics
        CN,  //  Consultation
        DI,  //  Diagnostic imaging
        DS,  //  Discharge summary
        ED,  //  Emergency department report
        HP,  //  History and physical examination
        OP,  //  Operative report
        PC,  //  Psychiatric consultation
        PH,  //  Psychiatric history and physical examination
        PN,  //  Procedure note
        PR,  //  Progress note
        SP,  //  Surgical pathology
        TS,  //  Transfer summary
        PDF  //  HCHB PDF types
    }
}
