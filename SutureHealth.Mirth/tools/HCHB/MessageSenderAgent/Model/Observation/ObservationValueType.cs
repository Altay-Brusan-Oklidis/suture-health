using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Observation
{
    /// <summary>
    /// HL7 Table - 0125 - Value type
    /// </summary>
    public enum ObservationValueType
    {
        AD , // Address
        CE , // Coded Entry
        CF , // Coded Element With Formatted Values
        CK , // Composite ID With Check Digit
        CN , // Composite ID And Name
        CP , // Composite Price
        CX , // Extended Composite ID With Check Digit
        DT , // Date
        ED , // Encapsulated Data
        FT , // Formatted Text (Display)	
        MO , // Money
        NM , // Numeric
        PN , // Person Name
        RP , // Reference Pointer
        SN , // Structured Numeric
        ST , // String Data.	
        TM , // Time
        TN , // Telephone Number
        TS , // Time Stamp (Date & Time)	
        TX , // Text Data (Display)	
        XAD, // Extended Address
        XCN, // Extended Composite Name And Number For Persons
        XON, // Extended Composite Name And Number For Organizations
        XPN, // Extended Person Name
        XTN, // Extended Telecommunications Number
    }
}
