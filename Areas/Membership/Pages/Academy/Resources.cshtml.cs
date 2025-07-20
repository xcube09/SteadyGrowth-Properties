using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Academy
{
    [Authorize(Policy = "KYCVerified")]
    public class ResourcesModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Academy", "/Membership/Academy/Courses") };
        }
    }
}
