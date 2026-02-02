using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Patient
{
    /// <summary>
    /// HL7 Table - 0005 - Race
    /// 
    /// This is demo race table.  
    /// </summary>
    public class RaceType
    {
        public static string AmericanNativeRace { get;} = "1002-5"; //American Indian or Alaska Native        
        public static string AsianRace {get;} = "2028-9";	         //Asian	
        public static string BlackRace {get;} = "2054-5";	         //Black or African American	
        public static string PacificRace { get;} = "2076-8";	     //Native Hawaiian or Other Pacific Islander	
        public static string WhiteRace {get;} = "2106-3";	         //White	
        public static string OtherRace { get; } = "2131-1";	         //Other Race
    }
}
