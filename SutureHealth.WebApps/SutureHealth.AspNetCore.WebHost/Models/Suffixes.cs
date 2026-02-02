using System.ComponentModel;

namespace SutureHealth.AspNetCore.Models
{
    public enum Suffixes
    {
        [Description("")]
        DEFAULT = 0,
        [Description("Sr.")]
        Sr = 10,
        [Description("Jr.")]
        Jr = 20,
        [Description("II")]
        II = 30,
        [Description("III")]
        III = 40,
        [Description("IV")]
        IV = 50,
        [Description("V")]
        V = 60,
        [Description("VI")]
        VI = 70,
        [Description("VII")]
        VII = 80,
        [Description("VIII")]
        VIII = 90,
        [Description("IX")]
        IX = 100,
        [Description("X")]
        X = 110
    }
}
