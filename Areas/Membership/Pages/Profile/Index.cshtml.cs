using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Users;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public IndexModel(ICurrentUserService currentUserService, IMediator mediator, IUserService userService)
        {
            _currentUserService = currentUserService;
            _mediator = mediator;
            _userService = userService;
        }

        [BindProperty]
        public UpdateUserProfileCommand? Command { get; set; }

        [BindProperty]
        public IFormFile? ProfilePicture { get; set; }

        public User? AppUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("My Profile", "/Membership/Profile/Index") };

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Login");
            }

            AppUser = await _userService.GetUserByIdAsync(userId);
            if (AppUser == null)
            {
                return NotFound();
            }

            Command = new UpdateUserProfileCommand
            {
                UserId = AppUser.Id,
                FirstName = AppUser.FirstName,
                LastName = AppUser.LastName,
                PhoneNumber = AppUser.PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Command == null)
            {
                return Page();
            }

            if (!_currentUserService.IsAuthenticated())
            {
                return RedirectToPage("/Identity/Login");
            }
            Command.UserId = _currentUserService.GetUserId();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Handle profile picture upload
            if (ProfilePicture != null)
            {
                var uploadPictureCommand = new UpdateUserProfilePictureCommand
                {
                    UserId = Command.UserId,
                    ProfilePicture = ProfilePicture
                };
                await _mediator.Send(uploadPictureCommand);
            }

            // Update user profile details
            var result = await _mediator.Send(Command);

            if (result)
            {
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error updating profile.");
                return Page();
            }
        }
    }

    public class UpdateUserProfileCommand : IRequest<bool>
    {
        public string? UserId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public string LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
    {
        private readonly UserManager<User> _userManager;

        public UpdateUserProfileCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return false;
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}