using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Authorization
{
    public class KYCRequirementHandler : AuthorizationHandler<KYCRequirement>
    {
        private readonly UserManager<User> _userManager;

        public KYCRequirementHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, KYCRequirement requirement)
        {
            var user = await _userManager.GetUserAsync(context.User);
            if (user != null && user.KYCStatus == KYCStatus.Approved)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}