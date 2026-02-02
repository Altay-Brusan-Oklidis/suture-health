using System;

namespace SutureHealth.Application
{
    public enum IdentityUseType
    {
        MultipleTimes,
        OneTime,
        PublicLogin,
    }

    public class PublicIdentity
    {
        public int PublicIdentityId { get; set; }
        public int MemberId { get; set; }
        public bool Active { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public IdentityUseType UseType { get; set; }
        public Guid Value { get; set; }
    }
}