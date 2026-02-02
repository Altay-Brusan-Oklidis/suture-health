using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.v0100.Models
{
   /// <summary>
   /// Class <see cref="Identifier"/> used for define the identifier
   /// </summary>
   [DataContract]
    public class Identifier : IIdentifier
    {
        [DataMember(Name = "type", EmitDefaultValue = false)]
        [JsonPropertyName("type")]
        [Required]
        public string Type { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        [JsonPropertyName("id")]
        [Required]
        public string Id { get; set; }

        string IIdentifier.Value { get { return this.Id; } set { this.Id = value; } }

        public bool Equals(IIdentifier other)
        {
            var fullId = this.Type + this.Id;
            return fullId.Equals(other.Type + other.Value);
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
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Gets or Sets the value of Created
        /// </summary>
        [DataMember(Name = "created", EmitDefaultValue = false)]
        [JsonPropertyName("created")]
        public DateTimeOffset Created { get; set; }
        /// <summary>
        /// Gets or Sets the value of Updated
        /// </summary>
        [DataMember(Name = "updated", EmitDefaultValue = false)]
        [JsonPropertyName("updated")]
        public DateTimeOffset? Updated { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Identifier<T> : Identifier
    {
        internal long ParentId { get; set; }
        internal T Parent { get; set; }
    }
}
