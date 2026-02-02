using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Visit
{


    public class ServiceLocationType
    {
       public static string? ASSISTED_LIVING_FACILITY { get; } = "ASSISTED LIVING FACILITY";
       public static string? HOME_CARE_IN_A_HOSPICE_FACILITY { get; } = "HOME CARE IN A HOSPICE FACILITY";
       public static string? INPATIENT_HOSPICE_FACILITY { get; } = "INPATIENT HOSPICE FACILITY";
       public static string? INPATIENT_HOSPITAL { get; } = "INPATIENT HOSPITAL";
       public static string? LONG_TERM_CARE_HOSPITAL { get; } = "LONG TERM CARE HOSPITAL";
       public static string? NURSING_LONG_TERM_CARE_OR_NON_SKILLED_NURSING_FACILITY { get; } = "NURSING LONG TERM CARE OR NON SKILLED NURSING FACILITY";
       public static string? PATIENTS_HOME_RESIDENCE { get; } = "PATIENT’S HOME/RESIDENCE";
       public static string? PLACE_NOT_OTHERWISE_SPECIFIED { get; } = "PLACE NOT OTHERWISE SPECIFIED";
       public static string? PSYCHIATRIC_FACILITY { get; } = "PSYCHIATRIC FACILITY";
       public static string? SKILLED_NURSING_FACIITY { get; } = "SKILLED NURSING FACIITY";
    }
}
