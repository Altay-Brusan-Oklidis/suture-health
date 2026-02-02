using System;

namespace SutureHealth.AspNetCore.SwaggerGen.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiKeyRequiredAttribute : Attribute
    {

    }
}
