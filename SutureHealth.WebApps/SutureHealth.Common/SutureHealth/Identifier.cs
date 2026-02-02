using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth
{
    public class Identifier : IIdentifier, IEquatable<IIdentifier>
    {
        public Identifier()
        { }

        [MaxLength(256)]
        public string Type { get; set; }
        [MaxLength(256)]
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as IIdentifier);
        }

        public bool Equals(IIdentifier identifier)
        {
            return identifier != null &&
                   Type.EqualsIgnoreCase(identifier.Type) &&
                   Value.EqualsIgnoreCase(identifier.Value);
        }

        public override int GetHashCode()
        {
            int hashCode = -1837957123;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Identifier {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append(" Value: ").Append(Value).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public class Identifier<TKeyType> : Identifier, IEquatable<Identifier<TKeyType>>
        where TKeyType : struct
    {
        public Identifier()
        { }

        public TKeyType Id { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Identifier<TKeyType>);
        }

        public bool Equals(Identifier<TKeyType> identifier)
        {
            return identifier != null &&
                   Id.Equals(identifier.Id) && 
                   Type.EqualsIgnoreCase(identifier.Type) &&
                   Value.EqualsIgnoreCase(identifier.Value);
        }

        public override int GetHashCode()
        {
            int hashCode = -1837957123;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Identifier {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append(" Value: ").Append(Value).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public class Identifier<TKeyType, TParentType, TParentKeyType> : Identifier<TKeyType>
        where TKeyType : struct
        where TParentType : class
        where TParentKeyType : struct
    {
        public TParentKeyType ParentId { get; set; }
        [JsonIgnore]
        public TParentType Parent { get; set; }
    }
}
