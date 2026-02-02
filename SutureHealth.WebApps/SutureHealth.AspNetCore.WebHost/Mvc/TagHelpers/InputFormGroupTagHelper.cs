using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SutureHealth.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("input-form-group")]
    public class InputFormGroupTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")] 
        public ModelExpression Source { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            var classNames = "input-form-group";
            if (output.Attributes.ContainsName("class"))
            {
                classNames = string.Format("{0} {1}", output.Attributes["class"].Value, classNames);
            }
            output.Attributes.SetAttribute("class", classNames);
            output.Content.AppendHtml(@"<div class=""form-group"">");
            output.Content.AppendHtml((await output.GetChildContentAsync()).GetContent());
            output.Content.AppendHtml("</div>");

            await base.ProcessAsync(context, output);
        }
    }
}
