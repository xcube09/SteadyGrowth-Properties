using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using SteadyGrowth.Web.Services.Interfaces;
using System.Security.Claims;

namespace SteadyGrowth.Web.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CurrentUserService> _logger;

        private const string ImpersonationSessionKey = "Admin_ImpersonatedUserId";
        private const string AdminUserSessionKey = "Admin_ActualUserId";

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            ApplicationDbContext context,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public string? GetUserId()
        {
            // Check if impersonating first
            var impersonatedUserId = _httpContextAccessor.HttpContext?.Session?.GetString(ImpersonationSessionKey);
            if (!string.IsNullOrEmpty(impersonatedUserId))
            {
                return impersonatedUserId;
            }

            // Return normal authenticated user
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                return _userManager.GetUserId(claimsPrincipal);
            }
            return null;
        }

        public string? GetUserEmail()
        {
            // If impersonating, get the impersonated user's email
            if (IsImpersonating())
            {
                var impersonatedUser = GetImpersonatedUserAsync().Result;
                return impersonatedUser?.Email;
            }

            // Return normal authenticated user email
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                return claimsPrincipal.FindFirstValue(ClaimTypes.Email);
            }
            return null;
        }

        public string? GetUserName()
        {
            // If impersonating, get the impersonated user's username
            if (IsImpersonating())
            {
                var impersonatedUser = GetImpersonatedUserAsync().Result;
                return impersonatedUser?.UserName;
            }

            // Return normal authenticated user name
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                return claimsPrincipal.Identity.Name;
            }
            return null;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<User?> GetCurrentUserWithDetailsAsync(bool includeWallet = false, bool includePackage = false)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var query = _context.Users.AsQueryable();

            if (includeWallet)
            {
                query = query.Include(u => u.Wallet);
            }

            if (includePackage)
            {
                query = query.Include(u => u.AcademyPackage);
            }

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public async Task<bool> IsInRoleAsync(string role)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> IsInAnyRoleAsync(params string[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                return false;
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return false;
            }

            foreach (var role in roles)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<Claim> GetUserClaims()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                return claimsPrincipal.Claims;
            }

            return Enumerable.Empty<Claim>();
        }

        public string? GetClaimValue(string claimType)
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                return claimsPrincipal.FindFirstValue(claimType);
            }

            return null;
        }

        public async Task<string?> GetUserFullNameAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return null;
            }

            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? user.UserName : fullName;
        }

        public async Task<bool> IsKycCompletedAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return false;
            }

            return user.KYCStatus == KYCStatus.Approved;
        }

        public async Task<string?> GetUserReferralCodeAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.ReferralCode;
        }

        public async Task<decimal> GetWalletBalanceAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            return wallet?.Balance ?? 0;
        }

        public async Task<bool> IsAdminAsync()
        {
            return await IsInRoleAsync("Admin");
        }

        public HttpContext? GetHttpContext()
        {
            return _httpContextAccessor.HttpContext;
        }

        public bool IsImpersonating()
        {
            var impersonatedUserId = _httpContextAccessor.HttpContext?.Session?.GetString(ImpersonationSessionKey);
            return !string.IsNullOrEmpty(impersonatedUserId);
        }

        public string? GetActualAdminUserId()
        {
            if (IsImpersonating())
            {
                return _httpContextAccessor.HttpContext?.Session?.GetString(AdminUserSessionKey);
            }
            return null;
        }

        public void StartImpersonation(string userId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var actualUserId = _userManager.GetUserId(httpContext.User);
                httpContext.Session.SetString(ImpersonationSessionKey, userId);
                httpContext.Session.SetString(AdminUserSessionKey, actualUserId ?? "");
            }
        }

        public void StopImpersonation()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.Remove(ImpersonationSessionKey);
                httpContext.Session.Remove(AdminUserSessionKey);
            }
        }

        public async Task<User?> GetImpersonatedUserAsync()
        {
            var impersonatedUserId = _httpContextAccessor.HttpContext?.Session?.GetString(ImpersonationSessionKey);
            if (!string.IsNullOrEmpty(impersonatedUserId))
            {
                return await _userManager.FindByIdAsync(impersonatedUserId);
            }
            return null;
        }
    }
}