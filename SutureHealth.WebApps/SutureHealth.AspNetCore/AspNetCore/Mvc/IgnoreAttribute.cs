using System;

namespace SutureHealth.AspNetCore.Mvc
{
    /// <summary>
    ///  Represents the class for custom ignore attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class IgnoreAttribute : Attribute
    {

    }
}
