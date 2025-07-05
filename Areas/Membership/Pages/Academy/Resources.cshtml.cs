using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Academy
{
    public class ResourcesModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Academy", "/Membership/Academy/Courses") };
        }
    }
}
