using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth.AspNetCore.Mvc
{
    /// <summary>
    /// A  custom filter that specifies the type of the value and status code returned by the action.
    /// </summary>
    public class ProducesResponseTypeAttribute : Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute
    {
        /// <summary>
        /// Gets or Sets examples of provider type.
        /// </summary>
        public Type ExamplesProviderType { get; set; }
        /// <summary>
        /// Gets json converter.
        /// </summary>
        public JsonConverter JsonConverter { get; }
        /// <summary>
        /// Gets or Sets ContractResolver.
        /// </summary>
        //public IContractResolver ContractResolver { get; }

        /// <summary>
        /// Used for calling the base constructor by numeric status code 
        /// </summary>
        /// <param name="statusCode">statusCode</param>
        public ProducesResponseTypeAttribute(int statusCode) : base(statusCode) { }

        /// <summary>
        /// Used for calling the base constructor by HttpStatusCode 
        /// </summary>
        /// <param name="statusCode">statusCode</param>
        public ProducesResponseTypeAttribute(System.Net.HttpStatusCode statusCode) : base((int)statusCode) { }

        /// <summary>
        /// Initialized the object.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="type"></param>
        /// <param name="exampleProviderType"></param>
        /// <param name="contractResolver"></param>
        /// <param name="jsonConverter"></param>
        public ProducesResponseTypeAttribute(System.Net.HttpStatusCode statusCode, Type type, Type exampleProviderType, Type contractResolver = null, Type jsonConverter = null) : base(type, (int)statusCode)
        {
            ExamplesProviderType = exampleProviderType;
            JsonConverter = jsonConverter == null ? null : (JsonConverter)Activator.CreateInstance(jsonConverter);
            //ContractResolver = contractResolver == null ? null : (IContractResolver)Activator.CreateInstance(contractResolver);
        }
    }
}
