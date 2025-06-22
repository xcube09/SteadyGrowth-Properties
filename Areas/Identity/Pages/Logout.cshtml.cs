using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<User> _signInManager;

    public LogoutModel(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task OnGet()
    {
        await _signInManager.SignOutAsync();
        // TODO: Clear authentication cookies if needed
        Response.Redirect("/");
    }
}
