using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Griesoft.AspNetCore.ReCaptcha;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/account")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IReferralService _referralService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountApiController> _logger;

        public AccountApiController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IReferralService referralService,
            ApplicationDbContext context,
            ILogger<AccountApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _referralService = referralService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                // Check if ModelState is valid (includes reCAPTCHA validation)
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );
                    return BadRequest(new { success = false, errors });
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { 
                        success = false, 
                        errors = new Dictionary<string, string[]> { 
                            ["Email"] = new[] { "Email is already registered." } 
                        } 
                    });
                }

                // Create new user
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                // Generate unique referral code
                string generatedReferralCode;
                do
                {
                    generatedReferralCode = GenerateReferralCode();
                } while (await _context.Users.AnyAsync(u => u.ReferralCode == generatedReferralCode));

                user.ReferralCode = generatedReferralCode;

                // Use transaction scope to coordinate UserManager and ApplicationDbContext operations
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Create user with password
                    var result = await _userManager.CreateAsync(user, model.Password);
                    
                    if (result.Succeeded)
                    {
                        // Assign Basic Package by default
                        var basicPackage = await _context.AcademyPackages
                            .FirstOrDefaultAsync(p => p.Name == "Basic Package");
                        
                        if (basicPackage != null)
                        {
                            user.AcademyPackageId = basicPackage.Id;
                            await _userManager.UpdateAsync(user);
                        }

                        // Process referral if provided
                        _logger.LogInformation("Processing registration for user {UserId}. Referral code provided: '{ReferralCode}'", 
                            user.Id, model.ReferralCode ?? "None");
                        
                        if (!string.IsNullOrWhiteSpace(model.ReferralCode))
                        {
                            _logger.LogInformation("Attempting to process referral with code {ReferralCode} for new user {UserId}", 
                                model.ReferralCode, user.Id);

                            // Verify user exists in ApplicationDbContext before processing referral
                            var userExistsInContext = await _context.Users.AnyAsync(u => u.Id == user.Id);
                            _logger.LogInformation("User {UserId} exists in ApplicationDbContext: {UserExists}", user.Id, userExistsInContext);

                            if (!userExistsInContext)
                            {
                                _logger.LogWarning("User {UserId} not found in ApplicationDbContext, attempting to refresh context", user.Id);
                                
                                // Try to find the user after a brief delay
                                await Task.Delay(100);
                                userExistsInContext = await _context.Users.AnyAsync(u => u.Id == user.Id);
                                _logger.LogInformation("User {UserId} exists in ApplicationDbContext after delay: {UserExists}", user.Id, userExistsInContext);
                            }

                            // Verify referrer exists
                            var referrerExists = await _context.Users.AnyAsync(u => u.ReferralCode == model.ReferralCode);
                            _logger.LogInformation("Referrer with code {ReferralCode} exists in ApplicationDbContext: {ReferrerExists}", 
                                model.ReferralCode, referrerExists);

                            if (!referrerExists)
                            {
                                _logger.LogWarning("Referrer with code {ReferralCode} not found in database", model.ReferralCode);
                            }
                            
                            var referralProcessed = await _referralService.ProcessReferralAsync(model.ReferralCode, user.Id);
                            if (!referralProcessed)
                            {
                                _logger.LogWarning("Failed to process referral for user {UserId} with code {ReferralCode}. UserExists={UserExists}, ReferrerExists={ReferrerExists}", 
                                    user.Id, model.ReferralCode, userExistsInContext, referrerExists);
                                // Note: We continue with registration even if referral fails
                            }
                            else
                            {
                                _logger.LogInformation("Successfully processed referral for user {UserId} with code {ReferralCode}", 
                                    user.Id, model.ReferralCode);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No referral code provided for user {UserId}", user.Id);
                        }

                        // Sign in the user
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        
                        // Complete the transaction
                        transactionScope.Complete();
                        
                        _logger.LogInformation("User {Email} registered successfully", model.Email);

                        return Ok(new { 
                            success = true, 
                            message = "Registration successful!",
                            redirectUrl = "/Membership/Dashboard/Index"
                        });
                    }
                    else
                    {
                        var errors = result.Errors.ToDictionary(
                            e => e.Code,
                            e => new[] { e.Description }
                        );
                        
                        return BadRequest(new { success = false, errors });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", model.Email);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred during registration. Please try again." 
                });
            }
        }

        private string GenerateReferralCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name must be less than 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be less than 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public string? ReferralCode { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? RecaptchaToken { get; set; }
    }
}