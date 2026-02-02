using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MessageSenderAgent.Model.Insurance
{
    /// <summary>
    /// HL7 Table - 0204 - Organizational name type
    /// </summary>
    public enum OrganizationNameTypeCode
    {
        A, // Alias name
        D, //  Display name
        L, // Legal name
        SL,//  Stock exchange listing name
    }
}
