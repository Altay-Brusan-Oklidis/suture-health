namespace SutureHealth.AspNetCore.Routing
{
    public class EnumerationRouteConstraint<TEnum> : IRouteConstraint
        where TEnum : System.Enum
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var candidate = values[routeKey]?.ToString();
            return Enum.TryParse(typeof(TEnum), candidate, true, out _);
        }
    }
}
