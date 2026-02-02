using NHapi.Base.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static NHapi.Base.Model.GenericMessage;

namespace SutureHealth.Hchb.Services.Testing.Model.Event
{
    /// <summary>
    /// HL7 Table - 0003 - Event Type
    /// <seealso cref="Header.TriggerEvent"/>
    /// </summary>
    public enum EventCodeType
    {       
        A01,    // ADT/ACK - Admit/visit notification
        A02,    // ADT/ACK - Transfer a Patient
        A03,    // ADT/ACK -  Discharge/end visit
        A04,    // ADT/ACK -  Register a Patient
        A05,    // ADT/ACK -  Pre-admit a Patient
        A06,    // ADT/ACK -  Change an outpatient to an inpatient
        A07,    // ADT/ACK -  Change an inpatient to an outpatient
        A08,    // ADT/ACK -  Update Patient information
        A09,    // ADT/ACK -  Patient departing - tracking
        A10,    // ADT/ACK -  Patient arriving - tracking
        A11,    // ADT/ACK -  Cancel admit/visit notification
        A12,    // ADT/ACK -  Cancel transfer
        A13,    // ADT/ACK -  Cancel discharge/end visit
        A14,    // ADT/ACK -  Pending admit
        A15,    // ADT/ACK -  Pending transfer
        A16,    // ADT/ACK -  Pending discharge
        A17,    // ADT/ACK -  Swap patients
        A18,    // ADT/ACK -  Merge Patient information (for backward compatibility only)	
        A19,    // QRY/ADR -  Patient query
        A20,    // ADT/ACK -  Bed Status update
        A21,    // ADT/ACK -  Patient goes on a “leave of absence”	
        A22,    // ADT/ACK -  Patient returns from a “leave of absence”	
        A23,    // ADT/ACK -  Delete a Patient record
        A24,    // ADT/ACK -  Link Patient information
        A25,    // ADT/ACK -  Cancel pending discharge
        A26,    // ADT/ACK -  Cancel pending transfer
        A27,    // ADT/ACK -  Cancel pending admit
        A28,    // ADT/ACK -  Add person information
        A29,    // ADT/ACK -  Delete person information
        A30,    // ADT/ACK -  Merge person information (for backward compatibility only)	
        A31,    // ADT/ACK -  Update person information
        A32,    // ADT/ACK -  Cancel Patient arriving - tracking
        A33,    // ADT/ACK -  Cancel Patient departing - tracking
        A34,    // ADT/ACK -  Merge Patient information - Patient ID only (for backward compatibility only)	
        A35,    // ADT/ACK -  Merge Patient information - account number only (for backward compatibility only)	
        A36,    // ADT/ACK -  Merge Patient information - Patient ID and account number (for backward compatibility only)	
        A37,    // ADT/ACK -  Unlink Patient information
        A38,    // ADT/ACK - Cancel pre-admit
        A39,    // ADT/ACK - Merge person - Patient ID (for backward compatibility only)	
        A40,    // ADT/ACK - Merge Patient - Patient identifier list
        A41,    // ADT/ACK - Merge account - Patient account number
        A42,    // ADT/ACK - Merge visit - visit number
        A43,    // ADT/ACK - Move Patient information - Patient identifier list
        A44,    // ADT/ACK - Move account information - Patient account number
        A45,    // ADT/ACK - Move visit information - visit number
        A46,    // ADT/ACK - Change Patient ID (for backward compatibility only)	
        A47,    // ADT/ACK - Change Patient identifier list
        A48,    // ADT/ACK - Change alternate Patient ID (for backward compatibility only)	
        A49,    // ADT/ACK - Change Patient account number
        A50,    // ADT/ACK - Change visit number
        A51,    // ADT/ACK - Change alternate visit ID
        C01,    // CRM - Register a Patient on a clinical trial
        C02,    // CRM - Cancel a Patient registration on clinical trial (for clerical mistakes only)	
        C03,    // CRM - Correct/update registration information
        C04,    // CRM - Patient has gone off a clinical trial
        C05,    // CRM - Patient enters phase of clinical trial
        C06,    // CRM - Cancel Patient entering a phase (clerical mistake)	
        C07,    // CRM - Correct/update phase information
        C08,    // CRM - Patient has gone off phase of clinical trial
        C09,    // CSU - Automated time intervals for reporting, like monthly
        C10,    // CSU - Patient completes the clinical trial
        C11,    // CSU - Patient completes a phase of the clinical trial
        C12,    // CSU - Update/correction of Patient order/result information
        CNQ,    // QRY/EQQ/VQQ/RQQ - Cancel query
        I01,    // RQI/RPI - Request for insurance information
        I02,    // RQI/RPL - Request/receipt of Patient selection display list
        I03,    // RQI/RPR - Request/receipt of Patient selection list
        I04,    // RQD/RPI - Request for Patient demographic data
        I05,    // RQC/RCI - Request for Patient clinical information
        I06,    // RQC/RCL - Request/receipt of clinical data listing
        I07,    // PIN/ACK - Unsolicited insurance information
        I08,    // RQA/RPA - Request for treatment authorization information
        I09,    // RQA/RPA - Request for modification to an authorization
        I10,    // RQA/RPA - Request for resubmission of an authorization
        I11,    // RQA/RPA - Request for cancellation of an authorization
        I12,    // REF/RRI -  Patient referral
        I13,    // REF/RRI - Modify Patient referral
        I14,    // REF/RRI - Cancel Patient referral
        I15,    // REF/RRI - Request Patient referral Status
        M01,    // MFN/MFK - Master file not otherwise specified ( for backward compatibility only )	
        M02,    // MFN/MFK - Master file - staff practitioner
        M03,    // MFN/MFK - Master file - test/observation ( for backward compatibility only )	
        M04,    // MFN/MFK - Master files charge description
        M05,    // MFN/MFK - Patient location master file
        M06,    // MFN/MFK - Clinical study with phases and schedules master file
        M07,    // MFN/MFK - Clinical study without phases but with schedules master file
        M08,    // MFN/MFK - Test/observation (numeric) master file
        M09,    // MFN/MFK - Test/observation (categorical) master file
        M10,    // MFN/MFK - Test /observation batteries master file
        M11,    // MFN/MFK - Test/calculated observations master file
        O01,    // ORM - Order Message (also RDE, RDS, RGV, RAS)	
        O02,    // ORR - Order response (also RRE, RRD, RRG, RRA)	
        P01,    // BAR/ACK - Add Patient accounts
        P02,    // BAR/ACK - Purge Patient accounts
        P03,    // DFT/ACK - Post detail financial Transaction
        P04,    // QRY/DSP - Generate bill and A/R statements
        P05,    // BAR/ACK - Update account
        P06,    // BAR/ACK - End account
        Q01,    // QRY/DSR - Query sent for immediate response
        Q02,    // QRY/QCK - Query sent for deferred response
        Q03,    // DSR/ACK - Deferred response to a query
        Q04,    // EQQ - Embedded query language query
        Q05,    // UDM/ACK - Unsolicited display update Message
        Q06,    // OSQ/OSR - Query for order Status
        R01,    // ORU/ACK - Unsolicited transmission of an observation Message
        R02,    // QRY - Query for results of observation
        R03,    // QRY/DSR Display-oriented results, query/unsol. update (for backward compatibility only) (Replaced by Q05)	
        R04,    // ORF - Response to query; transmission of requested observation
        R05,    // QRY/DSR - query for display results
        R06,    // UDM - unsolicited update/display results
        RAR,    // RAR - Pharmacy administration information query response
        RER,    // RER-Pharmacy encoded order information query response
        ROR,    // ROR - Pharmacy prescription order query response
        S01,    // SRM/SRR - Request new appointment booking
        S02,    // SRM/SRR - Request appointment rescheduling
        S03,    // SRM/SRR - Request appointment modification
        S04,    // SRM/SRR - Request appointment cancellation
        S05,    // SRM/SRR - Request appointment discontinuation
        S06,    // SRM/SRR - Request appointment deletion
        S07,    // SRM/SRR - Request addition of service/resource on appointment
        S08,    // SRM/SRR - Request modification of service/resource on appointment
        S09,    // SRM/SRR - Request cancellation of service/resource on appointment
        S10,    // SRM/SRR - Request discontinuation of service/resource on appointment
        S11,    // SRM/SRR - Request deletion of service/resource on appointment
        S12,    // SIU/ACK - Notification of new appointment booking
        S13,    // SIU/ACK - Notification of appointment rescheduling
        S14,    // SIU/ACK - Notification of appointment modification
        S15,    // SIU/ACK - Notification of appointment cancellation
        S16,    // SIU/ACK - Notification of appointment discontinuation
        S17,    // SIU/ACK - Notification of appointment deletion
        S18,    // SIU/ACK - Notification of addition of service/resource on appointment
        S19,    // SIU/ACK - Notification of modification of service/resource on appointment
        S20,    // SIU/ACK - Notification of cancellation of service/resource on appointment
        S21,    // SIU/ACK - Notification of discontinuation of service/resource on appointment
        S22,    // SIU/ACK - Notification of deletion of service/resource on appointment
        S23,    // SIU/ACK - Notification of blocked schedule time slot(s)
        S24,    // SIU/ACK - Notification of opened(“unblocked”) schedule time slot(s)
        S25,    // SQM/SQR - Schedule query Message and response
        S26,    // SIU/ACK Notification that Patient did not show up for schedule appointment
        T01,    // MDM/ACK - Original document notification
        T02,    // MDM/ACK - Original document notification and content
        T03,    // MDM/ACK - Document Status change notification
        T04,    // MDM/ACK - Document Status change notification and content
        T05,    // MDM/ACK - Document addendum notification
        T06,    // MDM/ACK - Document addendum notification and content
        T07,    // MDM/ACK - Document edit notification
        T09,    // MDM/ACK - Document cancel notification
        V01,    // VXQ - Query for vaccination record
        V02,    // VXX - Response to vaccination query returning multiple PID matches
        V03,    // VXR - Vaccination record response
        V04,    // VXU - Unsolicited vaccination record update
        Varies, //MFQ/MFR - Master files query(use event same as asking for e.g., M05 - location)
        W01,  //ORU - Waveform result, unsolicited transmission of requested information
        W02,  //QRF - Waveform result, response to query
    }
}
