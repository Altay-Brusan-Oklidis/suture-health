using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SutureHealth.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "if-matches-request")]
    public class MatchingActionTagHelper : TagHelper
    {
        public string IfMatchesRequest { get; set; }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            context.AllAttributes.TryGetAttribute("asp-area", out var htmlArea);
            context.AllAttributes.TryGetAttribute("asp-controller", out var htmlController);
            context.AllAttributes.TryGetAttribute("asp-action", out var htmlAction);
            context.AllAttributes.TryGetAttribute("asp-page", out var htmlPage);

            ViewContext.RouteData.Values.TryGetValue("area", out var actualArea);
            ViewContext.RouteData.Values.TryGetValue("controller", out var actualController);
            ViewContext.RouteData.Values.TryGetValue("action", out var actualAction);
            ViewContext.RouteData.Values.TryGetValue("page", out var actualPage);

            context.AllAttributes.TryGetAttribute("match-area", out var matchArea);
            context.AllAttributes.TryGetAttribute("match-controller", out var matchController);
            context.AllAttributes.TryGetAttribute("match-action", out var matchAction);
            context.AllAttributes.TryGetAttribute("match-url", out var matchUrl);
            context.AllAttributes.TryGetAttribute("match-page", out var matchPage);

            static bool? ProcessMatch(TagHelperAttribute matchItem, TagHelperAttribute tagItem, object? routeItem)
            {
                bool? isMatchingItem = null;
                if (bool.TryParse(matchItem?.Value.ToString(), out bool matchIt))
                    isMatchingItem = string.Equals(routeItem as string, tagItem?.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
                return isMatchingItem;
            }

            bool? isMatchingUrl = null,
                  isMatchingArea = ProcessMatch(matchArea, htmlArea, actualArea),
                  isMatchingController = ProcessMatch(matchController, htmlController, actualController),
                  isMatchingAction = ProcessMatch(matchAction, htmlAction, actualAction),
                  isMatchingPage = ProcessMatch(matchPage, htmlPage, actualPage);

            if (matchUrl != null && !string.IsNullOrWhiteSpace(matchUrl.Value.ToString()))
            {
                isMatchingUrl = System.Text.RegularExpressions.Regex.IsMatch(ViewContext.HttpContext.Request.Path, matchUrl.Value.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            if 
            (
                isMatchingUrl.GetValueOrDefault(true) && 
                isMatchingArea.GetValueOrDefault(true) && 
                isMatchingController.GetValueOrDefault(true) &&
                isMatchingAction.GetValueOrDefault(true) && 
                isMatchingPage.GetValueOrDefault(true)
            )
            {
                output.Attributes.SetAttribute("class", output.Attributes["class"].Value + " " + IfMatchesRequest);
            }

            return base.ProcessAsync(context, output);
        }
    }
}