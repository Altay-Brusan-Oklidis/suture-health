using Microsoft.VisualBasic;
using NHapi.Base.Model;
using NHapi.Model.V21.Datatype;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static NHapi.Base.Model.GenericMessage;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using System.Xml;

namespace SutureHealth.Hchb.Services.Testing.Model.Observation
{
    /// <summary>
    /// HL7 Table - 0396 - Coding system
    /// <warning>
    /// In the implementation some fields are renamed to make it consistant with C# enum definition
    /// <list Type="number">
    /// <item>
    /// <term>99zzzorL</term>
    /// <description>the original 99zzzorL is changed to _99zzzorL</description>
    /// </item>
    /// <item>
    /// <term>ANS+</term>
    /// <description>the original ANS+ is changed to ANS_P</description>
    /// </item>
    /// <item>
    /// <term>ISO+</term>
    /// <description>the original ISO+ is changed to ISO_P</description>
    /// </item>
    /// </warning>
    /// </summary>
    public enum NameofCodingSystem
    {
        /// <warning>
        /// the original 99zzzorL is changed to _99zzzorL
        /// </warning>
        _99zzzorL,  // Local general code (where z is an alphanumeric character)	Locally defined codes for purpose of Sender or receiver. Local codes can be identified by L (for backward compatibility) or 99zzz (where z is an alphanumeric character).
        ACR,        // American College of Radiology finding codes Index for Radiological Diagnosis Revised, 3rd Edition 1986, American College of Radiology, Reston, VA.
        /// <summary>
        /// The original ANS+ renamed to ANS_P
        /// </summary>
        ANS_P,       // HL7 set of units of measure HL7 set of units of measure based upon ANSI X3.50 - 1986, ISO 2988-83, and US customary units / see chapter 7, section 7.4.2.6.
        ART,        // WHO Adverse Reaction Terms  WHO Collaborating Centre for International Drug Monitoring, Box 26, S-751 03, Uppsala, Sweden.
        AS4,        // ASTM E1238/ E1467 Universal American Society for Testing & Materials and CPT4 (see Appendix X1 of Specification E1238 and Appendix X2 of Specification E1467).
        AS4E,       // AS4 Neurophysiology Codes   ASTM’s diagnostic codes and test result coding/grading systems for clinical neurophysiology. See ASTM Specification E1467, Appendix 2.
        ATC,        // American Type Culture Collection    Reference cultures (microorganisms, tissue cultures, etc.), related biological materials and associated data. American Type Culture Collection, 12301 Parklawn Dr, Rockville MD, 20852. (301) 881-2600. http://www.atcc.org
        C4,         // CPT-4	American Medical Association, P.O. Box 10946, Chicago IL  60610.
        C5,         // CPT-5	(under development - same contact as above)
        CAS,        // Chemical abstract codes These include unique codes for each unique chemical, including all generic drugs.The codes do not distinguish among different dosing forms. When multiple equivalent CAS numbers exist, use the first one listed in USAN.USAN 1990 and the USP dictionary of drug names, William M. Heller, Ph.D., Executive Editor, United States Pharmacopeial Convention, Inc., 12601 Twinbrook Parkway, Rockville, MD 20852.
        CD2,        // CDT-2 Codes American Dental Association’s Current Dental Terminology (CDT-2) code.American Dental Association, 211 E.Chicago Avenue,. Chicago, Illinois 60611.
        CDCA,       // CDC Analyte Codes   As above, for CDCM
        CDCM,       // CDC Methods/Instruments Codes   Public Health Practice Program Office, Centers for Disease Control and Prevention, 4770 Buford Highway, Atlanta, GA, 30421. Also available via FTP: ftp.cdc.gov/pub/laboratory _info/CLIA and Gopher: gopher.cdc.gov:70/11/laboratory_info/CLIA
        CDS,        // CDC Surveillance    CDC Surveillance Codes.For data unique to specific public health surveillance requirements.Epidemiology Program Office, Centers for Disease Control and Prevention, 1600 Clifton Rd, Atlanta, GA, 30333. (404) 639-3661.
        CE,         // CEN ECG diagnostic codes CEN PT007.A quite comprehensive set of ECG diagnostic codes(abbreviations) and descriptions published as a pre-standard by CEN TC251.Available from CEN TC251 secretariat, c/o Georges DeMoor, State University Hospital Gent, De Pintelaan 185-5K3, 9000 Gent, Belgium or Jos Willems, University of Gathuisberg, 49 Herestraat, 3000 Leuven, Belgium.
        CLP,        // CLIP Simon Leeming, Beth Israel Hospital, Boston MA. Codes for radiology reports.
        CPTM,       // CPT Modifier Code   Available for the AMA at the address listed for CPT above. These codes are found in Appendix A of CPT 2000 Standard Edition. (CPT 2000 Standard Edition, American Medical Association, Chicago, IL).
        CST,        // COSTART International coding system for adverse drug reactions.In the USA, maintained by the FDA, Rockville, MD.
        CVX,        // CDC Vaccine Codes   National Immunization Program, Centers for Disease Control and Prevention, 1660 Clifton Road, Atlanta, GA, 30333
        DCM,        // DICOM Controlled Terminology    Codes defined in DICOM Content Mapping Resource.Digital Imaging and Communications in Medicine (DICOM). NEMA Publication PS-3.16 National Electrical Manufacturers Association (NEMA). Rosslyn, VA, 22209. Available at: http://medical.nema.org
        E,          // EUCLIDES Available from Euclides Foundation International nv, Excelsiorlaan 4A, B-1930 Zaventem, Belgium; Phone: 32 2 720 90 60.
        E5,         // Euclides  quantity codes    Available from Euclides Foundation International nv(see above)
        E6,         // Euclides Lab method codes Available from Euclides Foundation International nv, Excelsiorlaan 4A, B-1930 Zaventem, Belgium; Phone : 32 2 720 90 60.
        E7,         // Euclides Lab equipment codes Available from Euclides Foundation International nv(see above)
        ENZC,       // Enzyme Codes Enzyme Committee of the International Union of Biochemistry and Molecular Biology.Enzyme Nomenclature: Recommendations on the Nomenclature and Classification of Enzyme-Catalysed Reactions. London: Academic Press, 1992.
        FDDC,       // First DataBank Drug Codes National Drug Data File.Proprietary product of First DataBank, Inc. (800) 633-3453, or http://www.firstdatabank.com.
        FDDX,       // First DataBank Diagnostic Codes Used for drug-diagnosis interaction checking.Proprietary product of First DataBank, Inc.As above for FDDC.
        FDK,        // FDA K10 Dept.of Health & Human Services, Food & Drug Administration, Rockville, MD 20857. (device & analyte process codes).
        HB,         // HIBCC   Health Industry Business Communications Council, 5110 N. 40th St., Ste 120, Phoenix, AZ 85018.
        HCPCS,      // CMS (formerly HCFA) Common Procedure Coding System HCPCS: contains codes for medical equipment, injectable drugs, transportation services, and other services not found in CPT4.
        HCPT,       // Health Care Provider Taxonomy The Blue Cross and Blue Shield Association will act as the administrator of the Provider Taxonomy so that the code structure is classified as external to X12.Ongoing maintenance is solely the responsibility of Workgroup 15 (Provider Information) within ANSI ASC X12N, or the work group’s successor.  Blue Cross and Blue Shield Association, 225 North Michigan Avenue, Chicago, IL 60601, Attention: ITS Department, ECNS Unit. http://www.wpc-edi.com/taxonomy/ Primary distribution is the responsibility of Washington Publishing Company, through its World Wide Web Site, at the same web site.
        HHC,        // Home Health Care    Home Health Care Classification System; Virginia Saba, EdD, RN; Georgetown University School of Nursing; Washington, DC.
        HI,         // Health Outcomes Health Outcomes Institute codes for outcome variables available(with responses) from Stratis Health(formerly Foundation for Health Care Evaluation and Health Outcomes Institute), 2901 Metro Drive, Suite 400, Bloomington, MN, 55425-1525; (612) 854-3306 (voice); (612) 853-8503 (fax); dziegen @winternet.com.See examples in the Implementation Guide.
        HL7nnnn,    // HL7 Defined Codes where nnnn is the HL7 table number    Health Level Seven where nnnn is the HL7 table number
        HOT,        // Japanese Nationwide Medicine Code
        HPC,        // CMS (formerly HCFA ) Procedure Codes (HCPCS) Health Care Financing Administration (HCFA) Common Procedure Coding System (HCPCS) including modifiers.[7]
        I10,        // ICD-10	World Health Publications, Albany, NY.
        I10P,       // ICD-10  Procedure Codes Procedure Coding System (ICD-10-PCS.)  See http://www/hcfa.gov/stats/icd10.icd10.htm for more information.
        I9,         // ICD9    World Health Publications, Albany, NY.
        I9C,        // ICD-9CM Commission on Professional and Hospital Activities, 1968 Green Road, Ann Arbor, MI 48105 (includes all procedures and diagnostic tests).
        IBT,        // ISBT    Retained for backward compatibility only as of v 2.5. This code value has been superceded by IBTnnnn.International Society of Blood Transfusion. Blood Group Terminology 1990. VOX Sanquines 1990 58(2):152-169.
        IBTnnnn,    // ISBT 128 codes where nnnn specifies a specific table within ISBT 128.	International Society of Blood Transfusion. (specific contact information will be supplied to editor.) The variable suffix(nnnn) identifies a specific table within ISBT 128.
        IC2,        // ICHPPC-2	International Classification of Health Problems in Primary Care, Classification Committee of World Organization of National Colleges, Academies and Academic Associations of General Practitioners(WONCA), 3rd edition.An adaptation of ICD9 intended for use in General Medicine, Oxford University Press.
        ICD10AM,    // ICD-10 Australian modification
        ICD10CA,    // ICD-10 Canada
        ICDO,       // International Classification of Diseases for Oncology International Classification of Diseases for Oncology, 2nd Edition. World Health Organization: Geneva, Switzerland, 1990. Order from: College of American Pathologists, 325 Waukegan Road, Northfield, IL, 60093-2750. (847) 446-8800.
        ICS,        // ICCS    Commission on Professional and Hospital Activities, 1968 Green Road, Ann Arbor, MI 48105.
        ICSD,       // International Classification of Sleep Disorders International Classification of Sleep Disorders Diagnostic and Coding Manual, 1990, available from American Sleep Disorders Association, 604 Second Street SW, Rochester, MN  55902
        /// <summary>
        /// The original ISO+ renamed to ISO_P
        /// </summary>
        ISO_P,       // ISO 2955.83 (units of measure) with HL7 extensions See chapter 7, section 7.4.2.6
        ISOnnnn,    // ISO Defined Codes where nnnn is the ISO table number    International Standards Organization where nnnn is the ISO table number
        IUPC,       // IUPAC/IFCC Component Codes Codes used by IUPAC/IFF to identify the component(analyte) measured.Contact Henrik Olesen, as above for IUPP.
        IUPP,       // IUPAC/IFCC Property Codes International Union of Pure and Applied Chemistry/International Federation of Clinical Chemistry.The Silver Book: Compendium of terminology and nomenclature of properties in clinical laboratory sciences.Oxford: Blackwell Scientific Publishers, 1995. Henrik Olesen, M.D., D.M.Sc., Chairperson, Department of Clinical Chemistry, KK76.4.2, Rigshospitalet, University Hospital of Copenhagen, DK-2200, Copenhagen.http://inet.uni-c.dk/~qukb7642/
        JC10,       // JLAC/JSLM, nationwide laboratory code Source: Classification &Coding for Clinical Laboratory.Japanese Society of Laboratory Medicine(JSLM, Old:Japan Society of Clinical Pathology). Version 10, 1997. A multiaxial code including a analyte code(e.g., Rubella = 5f395), identification code(e.g., virus ab IGG= 1431), a specimen code(e.g., serum = 023) and a method code(e.g., ELISA = 022)
        JC8,        // Japanese Chemistry Clinical examination classification code.Japan Association of Clinical Pathology.Version 8, 1990. A multiaxial code includ ing a subject code(e.g., Rubella = 5f395, identification code (e.g., virus ab IGG), a specimen code(e.g., serum = 023) and a method code(e.g., ELISA = 022)
        JJ1017,     // Japanese Image Examination Cache
        LB,         // Local billing code Local billing codes/names(with extensions if needed).
        LN,         // Logical Observation Identifier Names and Codes(LOINC®) Regenstrief Institute, c/o LOINC, 1050 Wishard Blvd., 5th floor, Indianapolis, IN  46202. 317/630-7433. Available from the Regenstrief Institute server at http://www.Regenstrief.org/loinc/loinc.htm. Also available via HL7 file server: FTP/Gopher (www.mcis.duke.edu/standards/ termcode/loinclab and www.mcis.duke.edu/standards/termcode/loinclin) and World Wide Web ( http://www.mcis.duke.edu/standards/termcode/loincl.htm ). January 2000 version has identifiers, synonyms and cross-reference codes for reporting over 26,000 laboratory and related observations and 1,500 clinical measures.
        MCD,        // Medicaid    Medicaid billing codes/names.
        MCR,        // Medicare    Medicare billing codes/names.
        MDDX,       // Medispan Diagnostic Codes   Codes Used for drug-diagnosis interaction checking.Proprietary product. Hierarchical drug codes for identifying drugs down to manufacturer and pill size. MediSpan, Inc., 8425 Woodfield Crossing Boulevard, Indianapolis, IN 46240. Tel: (800) 428-4495. WWW: http://www.espan.com/medispan/pages/medhome.html. As above for MGPI.
        MEDC,       // Medical Economics Drug Codes Proprietary Codes for identifying drugs.Proprietary product of Medical Economics Data, Inc. (800) 223-0581.
        MEDR,       // Medical Dictionary for Drug Regulatory Affairs(MEDDRA) Dr.Louise Wood, Medicines Control Agency, Market Towers, 1 Nine Elms Lane, London SW85NQ, UK   Tel: (44)0 171-273-0000 WWW:  http://www.open.gov.uk/mca/mcahome.htm
        MEDX,       // Medical Economics Diagnostic Codes Used for drug-diagnosis interaction checking.Proprietary product of Medical Economics Data, Inc. (800) 223-0581.
        MGPI,       // Medispan GPI Medispan hierarchical drug codes for identifying drugs down to manufacturer and pill size.Proprietary product of MediSpan, Inc., 8425 Woodfield Crossing Boulevard, Indianapolis, IN 46240. Tel: (800) 428-4495.
        MVX,        // CDC Vaccine Manufacturer Codes As above, for CVX
        NDA,       // NANDA North American Nursing Diagnosis Association, Philadelphia, PA.
        NDC,       // National drug codes These provide unique codes for each distinct drug, dosing form, manufacturer, and packaging. (Available from the National Drug Code Directory, FDA, Rockville, MD, and other sources.)
        NIC,       // Nursing Interventions Classification    Iowa Intervention Project, College of Nursing, University of Iowa, Iowa City, Iowa
        NPI,       // National Provider Identifier    Health Care Finance Administration, US Dept.of Health and Human Services, 7500 Security Blvd., Baltimore, MD 21244.
        NUBC,       //  National Uniform Billing Committee Code
        OHA,       // Omaha System Omaha Visiting Nurse Association, Omaha, NB.
        //OHA,       // Omaha   Omaha Visiting Nurse Association, Omaha, NB.
        POS,       // POS Codes HCFA Place of Service Codes for Professional Claims (see http://www.hcfa.gov/medicare/poscode.htm ).
        RC,       // Read Classification The Read Clinical Classification of Medicine, Park View Surgery, 26 Leicester Rd., Loughborough LE11 2AG (includes drug procedure and other codes, as well as diagnostic codes).
        SDM,        // SNOMED- DICOM Microglossary College of American Pathologists, Skokie, IL, 60077-1034. (formerly designated as 99SDM).
        SNM,        // Systemized Nomenclature of Medicine(SNOMED)    Systemized Nomenclature of Medicine, 2nd Edition 1984 Vols 1, 2, College of American Pathologists, Skokie, IL.
        SNM3,       // SNOMED International SNOMED International, 1993 Vols 1-4, College of American Pathologists, Skokie, IL, 60077-1034.
        SNT,        // SNOMED topology codes(anatomic sites)  College of American Pathologists, 5202 Old Orchard Road, Skokie, IL 60077-1034.
        UC,         // UCDS    Uniform Clinical Data Systems.Ms.Michael McMullan, Office of Peer Review Health Care Finance Administration, The Meadows East Bldg., 6325 Security Blvd., Baltimore, MD 21207; (301) 966 6851.
        UMD,        // MDNS    Universal Medical Device Nomenclature System.ECRI, 5200 Butler Pike, Plymouth Meeting, PA  19462 USA.Phone: 215-825-600 0, Fax: 215-834-1275.
        UML,        // Unified Medical Language    National Library of Medicine, 8600 Rockville Pike, Bethesda, MD 20894.
        UPC,        // Universal Product Code  The Uniform Code Council. 8163 Old Yankee Road, Suite J, Dayton, OH  45458; (513) 435 3070
        UPIN,       // UPIN    Medicare/CMS 's (formerly HCFA)  universal physician identification numbers, available from Health Care Financing Administration, U.S. Dept. of Health and Human Services, Bureau of Program Operations, 6325 Security Blvd., Meadows East Bldg., Room 300, Baltimore, MD 21207
        USPS,       // United States Postal Service Two Letter State and Possession Abbreviations are listed in  Publication 28, Postal Addressing Standards which can be obtained from Address Information Products, National Address Information Center, 6060 Primacy Parkway, Suite 101, Memphis, Tennessee  38188-0001 Questions of comments regarding the publication should be addressed to the Office of Address and Customer Information Systems, Customer and Automation Service Department, US Postal Service, 475 Lenfant Plaza SW Rm 7801, Washington, DC  20260-5902
        W1,         // WHO record # drug codes (6 digit)	World Health organization record number code. A unique sequential number is assigned to each unique single component drug and to each multi-component drug. Eight digits are allotted to each such code, six to identify the active agent, and 2 to identify the salt, of single content drugs. Six digits are assigned to each unique combination of drugs in a dispensing unit. The six digit code is identified by W1, the 8 digit code by W2.
        W2,         //  WHO record # drug codes (8 digit)	World Health organization record number code. A unique sequential number is assigned to each unique single component drug and to each multi-component drug. Eight digits are allotted to each such code, six to identify the active agent, and 2 to identify the salt, of single content drugs. Six digits are assigned to each unique combination of drugs in a dispensing unit. The six digit code is identified by W1, the 8 digit code by W2.
        W4,         // WHO record # code with ASTM extension	With ASTM extensions (see Implementation Guide), the WHO codes can be used to report serum (and other) levels, Patient compliance with drug usage instructions, average daily doses and more (see Appendix X1 the Implementation Guide).
        WC,         //  WHO ATC WHO’s ATC codes provide a hierarchical classification of drugs by therapeutic class. They are linked to the record number codes listed above.
    }
}
