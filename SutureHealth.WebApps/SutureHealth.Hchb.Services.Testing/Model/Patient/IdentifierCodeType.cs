using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks ;

namespace SutureHealth.Hchb.Services.Testing.Model.Patient 
{
    /// <summary>
    /// HL7 Table - 0203 - Identifier Type
    /// </summary>
    [Serializable]
    public enum IdentifierCodeType
    {
        AM  ,   //American Express    Deprecated and replaced by BC in v 2.5.
        AN  ,   //Account number  An identifier that is unique to an account.
        ANC ,   //Account number Creditor Class: Financial A more precise definition of an account number: sometimes two distinct account numbers must be transmitted in the same Message, one as the creditor, the other as the debitor. Kreditorenkontonummer
        AND ,   //Account number debitor  Class: Financial A more precise definition of an account number: sometimes two distinct account numbers must be transmitted in the same Message, one as the creditor, the other as the debitor. Debitorenkontonummer
        ANON,   //Anonymous identifier    An identifier for a living subject whose real identity is protected or suppressed Justification: For public health reporting purposes, anonymous identifiers are occasionally used for protecting Patient identity in reporting certain results.For instance, a state health department may choose to use a scheme for generating an anonymous identifier for reporting a Patient that has had a positive human immunodeficiency virus antibody test.Anonymous identifiers can be used in PID 3 by replacing the medical record number or other non-anonymous identifier.The assigning authority for an anonymous identifier would be the state/local health department.
        ANT ,   //Temporary Account Number    Class: FinancialTemporary version of an Account Number. Use Case: An ancillary system that does not normally assign account numbers is the first time to register a Patient.This ancillary system will generate a temporary account number that will only be used until an official account number is assigned.
        APRN,   //Advanced Practice Registered Nurse number   An identifier that is unique to an advanced practice registered nurse within the jurisdiction of a certifying board
        BA ,    //Bank Account Number Class: Financial
        BC  ,   //Bank Card Number Class: Financial An identifier that is unique to a person’s bank card.Replaces AM, DI, DS, MS, and VS beginning in v 2.5.
        BR ,    //Birth registry number
        BRN ,   //Breed Registry Number
        CC ,    //Cost Center number  Class: Financial Use Case: needed especially for transmitting information about invoices.
        CY ,    //County number
        DDS ,   //Dentist license number An identifier that is unique to a dentist within the jurisdiction of the licensing board
        DEA ,   //Drug Enforcement Administration registration number An identifier for an individual or organization relative to controlled substance regulation and transactions.Use case: This is a registration number that identifies an individual or organization relative to controlled substance regulation and transactions. A DEA number has a very precise and widely accepted meaning within the United States.Surprisingly, the US Drug Enforcement Administration does not solely assign DEA numbers in the United States.Hospitals have the authority to issue DEA numbers to their medical residents. These DEA numbers are based upon the hospital’s DEA number, but the authority rests with the hospital on the assignment to the residents.Thus, DEA as an Identifier Type is necessary in addition to DEA as an Assigning Authority.
        DFN ,   //Drug Furnishing or prescriptive authority Number An identifier issued to a health care provider authorizing the person to write drug orders Use Case: A nurse practitioner has authorization to furnish or prescribe pharmaceutical substances; this identifier is in component 1.
        DI ,    //Diner’s Club card Deprecated and replaced by BC in v 2.5.
        DL ,    //Driver’s license number
        DN  ,   //Doctor number
        DO ,    //Osteopathic License number  An identifier that is unique to an osteopath within the jurisdiction of a licensing board.
        DPM ,   //Podiatrist license number   An identifier that is unique to a podiatrist within the jurisdiction of the licensing board.
        DR ,    //Donor Registration Number
        DS ,    //Discover Card Deprecated and replaced by BC in v 2.5.
        EI ,    //Employee number A number that uniquely identifies an employee to an employer.
        EN ,    //Employer number
        FI  ,   //Facility ID
        GI ,    //Guarantor internal identifier Class: Financial
        GL  ,   //General ledger number Class: Financial
        GN  ,   //Guarantor external  identifier Class: Financial
        HC  ,   //Health Card Number
        IND ,   //Indigenous/Aboriginal A number assigned to a member of an indigenous or aboriginal group outside of Canada.
        JHN ,   //Jurisdictional health number (Canada) Class: Insurance 2 uses: a) UK jurisdictional CHI number; b) Canadian provincial health card number:
        LI ,    //Labor and industries number
        LN  ,   //License number
        LR ,    //Local Registry ID
        MA ,    //Patient Medicaid number Class: Insurance
        MB  ,   //Member Number   An identifier for the insured of an insurance policy(this insured always has a subscriber), usually assigned by the insurance carrier.Use Case: Person is covered by an insurance policy.This person may or may not be the subscriber of the policy.
        MC ,    //Patient's Medicare number	Class: Insurance
        MCD ,   //Practitioner Medicaid number    Class: Insurance
        MCN ,   //Microchip Number
        MCR ,   //Practitioner Medicare number    Class: Insurance
        MD  ,   //Medical License number An identifier that is unique to a medical doctor within the jurisdiction of a licensing board. Use Case: These license numbers are sometimes used as identifiers.In some states, the same authority issues all three identifiers, e.g., medical, osteopathic, and physician assistant licenses all issued by one state medical board.For this case, the CX data Type requires distinct identifier types to accurately interpret component 1. Additionally, the distinction among these license types is critical in most health care settings (this is not to convey full licensing information, which requires a segment to support all related attributes).
        MI ,    //Military ID number  A number assigned to an individual who has had military duty, but is not currently on active duty.The number is assigned by the DOD or Veterans’ Affairs(VA).
        MR ,    //Medical record number   An identifier that is unique to a Patient within a set of medical records, not necessarily unique within an application.
        MRT ,   //Temporary Medical Record Number Temporary version of a Medical Record Number Use Case: An ancillary system that does not normally assign medical record numbers is the first time to register a Patient.This ancillary system will generate a temporary medical record number that will only be used until an official medical record number is assigned.
        MS ,    //MasterCard  Deprecated and replaced by BC in v 2.5.
        NE ,    //National employer identifier    In the US, the Assigning Authority for this value is typically CMS, but it may be used by all providers and insurance companies in HIPAA related transactions.
        NH ,    //National Health Plan Identifier Class: InsuranceUsed for the UK NHS national identifier.In the US, the Assigning Authority for this value is typically CMS, but it may be used by all providers and insurance companies in HIPAA related transactions.
        NI ,    //National unique individual identifier Class: Insurance In the US, the Assigning Authority for this value is typically CMS, but it may be used by all providers and insurance companies in HIPAA related transactions.
        NII ,   //National Insurance Organization Identifier Class: Insurance In Germany a national identifier for an insurance company.It is printed on the insurance card (health card). It is not to be confused with the health card number itself. Krankenkassen-ID der KV-Karte
        NIIP,   //National Insurance Payor Identifier (Payor) Class: Insurance In Germany the insurance identifier addressed as the payor. Krankenkassen-ID des Rechnungsempfängers Use case: a subdivision issues the card with their identifier, but the main division is going to pay the invoices.
        NNxxx,  // National Person Identifier where the xxx is the ISO table 3166 3-character (alphabetic) country code
        NP  ,   //Nurse practitioner number An identifier that is unique to a nurse practitioner within the jurisdiction of a certifying board.
        NPI ,   //National provider identifier    Class: Insurance In the US, the Assigning Authority for this value is typically CMS, but it may be used by all providers and insurance companies in HIPAA related transactions.
        OD ,    //Optometrist license number  A number that is unique to an individual optometrist within the jurisdiction of the licensing board.
        PA ,    //Physician Assistant number  An identifier that is unique to a physician assistant within the jurisdiction of a licensing board
        PCN ,   //Penitentiary/correctional institution Number A number assigned to individual who is incarcerated.
        PE ,    //Living Subject Enterprise Number An identifier that is unique to a living subject within an enterprise (as identified by the Assigning Authority).
        PEN ,   //Pension Number
        PI ,    //Patient internal identifier A number that is unique to a Patient within an Assigning Authority.
        PN  ,   //Person number   A number that is unique to a living subject within an Assigning Authority.
        PNT ,   //Temporary Living Subject Number Temporary version of a Lining Subject Number.
        PPN ,   //Passport number A unique number assigned to the document affirming that a person is a citizen of the country.In the US this number is issued only by the State Department.
        PRC ,   //Permanent Resident Card Number
        PRN ,   //Provider number A number that is unique to an individual provider, a provider group or an organization within an Assigning Authority. Use case: This allows PRN to represent either an individual (a nurse) or a group/organization (orthopedic surgery team).
        PT ,    //Patient external identifier
        QA ,    //QA number
        RI  ,   //Resource identifier A generalized resource identifier.Use Case: An identifier Type is needed to accommodate what are commonly known as resources.The resources can include human (e.g.a respiratory therapist), non-human(e.g., a companion animal), inanimate object (e.g., an exam room), organization(e.g., diabetic education class) or any other physical or logical entity.
        RN ,    //Registered Nurse Number An identifier that is unique to a registered nurse within the jurisdiction of the licensing board.
        RPH ,   //Pharmacist license number An identifier that is unique to a pharmacist within the jurisdiction of the licensing board.
        RR ,    //Railroad Retirement number
        RRI ,   //Regional registry ID
        SL ,    //State license
        SN ,    //Subscriber Number Class: Insurance An identifier for a subscriber of an insurance policy which is unique for, and usually assigned by, the insurance carrier.Use Case: A person is the subscriber of an insurance policy. The person’s family may be plan members, but are not the subscriber.
        SR ,    //State registry ID
        SS ,    //Social Security number
        TAX ,   //Tax ID number
        TN ,    //Treaty Number/ (Canada) A number assigned to a member of an indigenous group in Canada.Use Case: First Nation.
        U ,     //Unspecified identifier
        UPIN,   //Medicare/CMS (formerly HCFA)’s Universal Physician Identification numbers Class: Insurance
        VN  ,   //Visit number
        VS ,    //VISA Deprecated and replaced by BC in v 2.5.
        WC ,    //WIC identifier
        WCN ,   //Workers’ Comp Number
        XX  ,    //Organization identifier
        SS4      // Social Secuirty Serial 

    }
}
