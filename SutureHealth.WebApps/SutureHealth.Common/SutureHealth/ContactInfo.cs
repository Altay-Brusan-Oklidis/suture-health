using System;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth
{
    public class ContactInfo<TKeyType> : IContactInfo
        where TKeyType : struct
    {
        public TKeyType Id { get; set; }
        
        public ContactType Type { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ContactInfo {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Value: ").Append(Value).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public abstract class ContactInfo<TKeyType, TParentType, TParentKeyType> : ContactInfo<TKeyType>
        where TKeyType : struct
        where TParentType : class
        where TParentKeyType : struct
    {
        public TParentKeyType ParentId { get; set; }
        [JsonIgnore]
        public TParentType Parent { get; set; }
    }
}
