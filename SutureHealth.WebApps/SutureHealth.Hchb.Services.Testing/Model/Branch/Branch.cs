using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Branch
{
    [Serializable]
    public class Branch
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int FacilityId { get; set; }

    }
}
