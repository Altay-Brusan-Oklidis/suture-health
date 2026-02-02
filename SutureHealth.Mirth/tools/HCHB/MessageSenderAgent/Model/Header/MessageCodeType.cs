using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Header
{
    /// <summary>
    /// HL7 Table - 0076 - Message type
    /// <remarks> Some fields are commented out, due to repetition</remarks>
    /// </summary>
    public enum MessageCodeType
    {
        ACK, // General acknowledgment message
        ADR, // ADT response
        ADT, // ADT message
        ARD, // Ancillary RPT (display)	
        BAR, // Add/change billing account
        CNQ, // Cancel query
        CSU, // Unsolicited clinical study data
        DFT, // Detail financial transaction
        DSR, // Display response
        EDR, // Enhanced display response
        EQQ, // Embedded query language query
        ERP, // Event replay response
        ERQ, // Event replay query
        MCF, // Delayed acknowledgment
        MDM, // Documentation message
        MFD, // Master files delayed application acknowledgment
        MFK, // Master files application acknowledgment
        MFN, // Master files notification
        MFQ, // Master files query
        MFR, // Master files query response
        ORF, // Observation result/record response
        ORM, // Order message
        ORR, // Order acknowledgment message
        ORU, // Observation result/unsolicited
        OSQ, // Order status query
        OSR, // Order status response
        PEX, // Product experience
        PGL, // Patient goal
        PGQ, // Patient goal query
        PGR, // Patient goal response
        PIN, // Patient Insurance Information or Patient information
        //PIN, // Patient information
        PPP, // Patient pathway (problem-oriented)	
        PPR, // Patient problem or Patient problem
        //PPR, // Patient problem
        PPT, // Patient pathway (goal oriented)	
        PPV, // Patient goal response
        PRQ, // Patient care problem query
        PRR, // Patient problem response
        PTQ, // Patient pathway (problem-oriented) query
        PTR, // Patient pathway (problem-oriented) response
        PTU, // Patient pathway (goal-oriented) query
        PTV, // Patient pathway (goal-oriented) response
        QRY, // Query
        RAR, // Pharmacy administration information
        RAS, // Pharmacy administration message
        RCI, // Return clinical information
        RCL, // Return clinical list
        RDE, // Pharmacy encoded order message
        RDR, // Pharmacy dispense information
        RDS, // Pharmacy dispense message
        REF, // Patient referral
        RER, // Pharmacy encoded order information
        RGR, // Pharmacy dose information
        RGV, // Pharmacy give message
        ROC, // Request clinical information
        ROD, // Request patient demographics
        ROR, // Pharmacy prescription order response
        RPA, // Return patient authorization
        RPI, // Return patient information
        RPL, // Return patient display list
        RPR, // Return patient list
        RQA, // Request patient authorization
        RQI, // Request patient information
        RRA, // Pharmacy administration acknowledgment
        RRD, // Pharmacy dispense acknowledgment
        RRE, // Pharmacy encoded order acknowledgment
        RRG, // Pharmacy give acknowledgment
        RRI, // Return patient referral
        SIU, // Schedule information unsolicited
        SPQ, // Stored procedure request
        SQM, // Schedule query
        SQR, // Schedule query response
        SRM, // Study registration or Schedule request
        //SRM, // Schedule request
        SRR, // Scheduled request response
        TBR, // Tabular response
        UDM, // Unsolicited display message
        VQQ, // Virtual table query
        VXQ, // Query for vaccination record
        VXR, // Vaccination query record response
        VXU, // Unsolicited vaccination record update
        VXX, //
    }
}
