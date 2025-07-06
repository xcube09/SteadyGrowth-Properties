using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SteadyGrowth.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "sg-readonly-if")]
    public class ReadOnlyTagHelper : TagHelper
    {
        [HtmlAttributeName("sg-readonly-if")]
        public bool Condition { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Condition)
            {
                output.Attributes.SetAttribute("readonly", "readonly");
            }
        }
    }
}
