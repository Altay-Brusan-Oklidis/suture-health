using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application
{
    public class FeatureFlagDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

}
