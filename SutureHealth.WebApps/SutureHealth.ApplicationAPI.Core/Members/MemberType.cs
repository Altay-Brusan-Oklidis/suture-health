using System.ComponentModel;

namespace SutureHealth.Application
{
    public enum MemberType : int
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Physician")]
        Physician = 2000,
        [Description("Nurse Practitioner")]
        NursePractitioner = 2001,
        [Description("Nurse")]
        Nurse = 2002,
        [Description("Staff")]
        Staff = 2003,
        [Description("Physician Assistant")]
        PhysicianAssistant = 2008,
        [Description("Clerical Support Staff")]
        ClericalSupportStaff = 2012,
        [Description("Application Admin")]
        ApplicationAdmin = 2016,
        [Description("API Integration")]
        ApiIntegration = 2017
    }

    public static class MemberTypes
    {
        public static int[] Assistants = new int[] { 2001, 2002, 2008, 2012, 2015 };
    }
}
