using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Visit
{
    /// <summary>
    /// HL7 Table - 0360 - Degree/license/certificate
    /// MT is repeated and one of them is removed.
    /// </summary>
    public enum DegreeLicenseCertificateType
    {
        AA,  // Associate of Arts
        AAS, // Associate of Applied Science
        ABA, // Associate of Business Administration
        AE , // Associate of Engineering
        AS , // Associate of Science
        BA , // Bachelor of Arts
        BBA, // Bachelor of Business Administration
        BE , // Bachelor or Engineering
        BFA, // Bachelor of Fine Arts
        BN , // Bachelor of Nursing
        BS , // Bachelor of Science
        BSL, // Bachelor of Science - Law
        BSN, // Bachelor on Science - Nursing
        BT , // Bachelor of Theology
        CANP,// Certified Adult Nurse Practitioner
        CER, // Certificate
        CMA, // Certified Medical Assistant
        CNM, // Certified Nurse Midwife
        CNP, // Certified Nurse Practitioner
        CNS, // Certified Nurse Specialist
        CPNP,// Certified Pediatric Nurse Practitioner
        CRN, // Certified Registered Nurse
        DBA, // Doctor of Business Administration
        DED, // Doctor of Education
        DIP, // Diploma
        DO , // Doctor of Osteopathy
        EMT, // Emergency Medical Technician
        EMTP,// Emergency Medical Technician - Paramedic
        FPNP,// Family Practice Nurse Practitioner
        HS , // High School Graduate
        JD , // Juris Doctor
        MA , // Master of Arts
        MBA, // Master of Business Administration
        MCE, // Master of Civil Engineering
        MD , // Doctor of Medicine
        MDA, // Medical Assistant
        MDI, // Master of Divinity
        ME , // Master of Engineering
        MED, // Master of Education
        MEE, // Master of Electrical Engineering
        MFA, // Master of Fine Arts
        MME, // Master of Mechanical Engineering
        MS , // Master of Science
        MSL, // Master of Science - Law
        MSN, // Master of Science - Nursing
        MT , // Master of Theology
        //MT , // Medical Technician
        NG , // Non-Graduate
        NP , // Nurse Practitioner
        PA , // Physician Assistant
        Pha, //rmD  Doctor of Pharmacy
        PHD, // Doctor of Philosophy
        PHE, // Doctor of Engineering
        PHS, // Doctor of Science
        PN , // Advanced Practice Nurse
        RMA, // Registered Medical Assistant
        RPH, // Registered Pharmacist
        SEC, // Secretarial Certificate
        TS , // Trade School Graduate
    }
}
