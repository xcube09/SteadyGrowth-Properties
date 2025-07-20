using Microsoft.AspNetCore.Authorization;

namespace SteadyGrowth.Web.Authorization
{
    public class KYCRequirement : IAuthorizationRequirement
    {
        public KYCRequirement() { }
    }
}