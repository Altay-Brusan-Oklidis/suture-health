using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Address
{
    /// <summary>
    /// HL7 Table - 0190 - Address type
    /// 
    /// HL7 Standard Tables - Tables defined by the HL7 organization. 
    /// Most tables in this category are related to the standard and 
    /// its message structures.
    /// </summary>
    public enum FascilityType
    {
        B,// Firm/Business
        BA,// Bad address
        BDL,// Birth delivery location (address where birth occurred)	
        BR,// Residence at birth (home address at time of birth)	
        C,// Current Or Temporary
        F,// Country Of Origin
        H,// Home
        L,// Legal Address
        M,// Mailing
        N,// Birth (nee) (birth address, not otherwise specified)	
        O,// Office
        P,// Permanent
        RH // Registry home. Refers to the information system, typically managed by a public health agency, that stores patient information such as immunization histories or cancer data, regardless of where the patient obtains services.
    }
}
