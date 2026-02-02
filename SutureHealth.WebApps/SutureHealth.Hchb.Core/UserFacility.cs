using System;

namespace SutureHealth.Hchb
{
    public class UserFacility
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FacilityId { get; set; }
        public Boolean Primary { get; set; }
        public Boolean Active { get; set; }
    }
}
