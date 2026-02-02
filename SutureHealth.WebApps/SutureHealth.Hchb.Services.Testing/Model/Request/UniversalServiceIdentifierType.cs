using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Request
{
    public class UniversalServiceIdentifierType
    {
        [MaxLength(20)]
        public string? Identifier { get; set; }
        [MaxLength(199)]
        public string? Text { get; set; }
    }
}
