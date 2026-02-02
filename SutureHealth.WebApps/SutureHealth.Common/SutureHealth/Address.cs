using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth
{
    public class Address<T> : IAddress
    {
        public T Id { get; set; }

        [MaxLength(150)]
        public string Line1 { get; set; }
        [MaxLength(150)]
        public string Line2 { get; set; }
        [MaxLength(150)]
        public string City { get; set; }
        [MaxLength(50)]
        public string County { get; set; }
        [MaxLength(2)]
        public string StateOrProvince { get; set; }
        [MaxLength(50)]
        public string CountryOrRegion { get; set; }
        [MaxLength(9)]
        public string PostalCode { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Address<T>);
        }

        public bool Equals(Address<T> other)
        {
            return other != null &&
                   Line1 == other.Line1 &&
                   Line2 == other.Line2 &&
                   City == other.City &&
                   County == other.County &&
                   StateOrProvince == other.StateOrProvince &&
                   CountryOrRegion == other.CountryOrRegion &&
                   PostalCode == other.PostalCode;
        }

        public override int GetHashCode()
        {
            int hashCode = 348310844;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Line1);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Line2);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(City);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(County);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StateOrProvince);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CountryOrRegion);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PostalCode);
            return hashCode;
        }
    }

    public class Address<TKeyType,TParentType,TParentKeyType> : Address<TKeyType>
        where TKeyType : struct
        where TParentType : class
        where TParentKeyType : struct
    {
        public TParentKeyType ParentId { get; set; }
        [JsonIgnore]
        public TParentType Parent { get; set; }
    }
}
