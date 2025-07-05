using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Properties;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Application.ViewModels;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties
{
    public class EditModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;

        public EditModel(IMediator mediator, IWebHostEnvironment webHostEnvironment, UserManager<User> userManager)
        {
            _mediator = mediator;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [BindProperty]
        public UpdatePropertyCommand? Command { get; set; }

        [BindProperty]
        public List<PropertyImageUploadModel> NewImages { get; set; } = new List<PropertyImageUploadModel>();

        [BindProperty]
        public List<PropertyImageCommandDto> ExistingImages { get; set; } = new List<PropertyImageCommandDto>();

        [BindProperty]
        public List<int> ImagesToDelete { get; set; } = new List<int>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("My Properties", "/Membership/Properties/Index"), ("Edit Property", $"/Membership/Properties/Edit/{id}") };

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            var query = new GetPropertyDetailsQuery { PropertyId = id, UserId = userId };
            var property = await _mediator.Send(query);

            if (property == null)
            {
                return NotFound();
            }

            Command = new UpdatePropertyCommand
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                UserId = property.UserId
            };

            ExistingImages = property.PropertyImages.Select(pi => new PropertyImageCommandDto
            {
                Id = pi.Id,
                FileName = pi.FileName,
                Caption = pi.Caption,
                ImageType = pi.ImageType,
                DisplayOrder = pi.DisplayOrder
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Command == null)
            {
                return Page();
            }

            if (User.Identity == null || User.Identity.Name == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }
            Command.UserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Handle new image uploads
            foreach (var image in NewImages)
            {
                if (image.ImageFile != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.ImageFile.CopyToAsync(fileStream);
                    }

                    Command.NewImages.Add(new PropertyImageCommandDto
                    {
                        FileName = uniqueFileName,
                        Caption = image.Caption,
                        ImageType = image.ImageType,
                        DisplayOrder = image.DisplayOrder
                    });
                }
            }

            // Handle existing images (updates and deletions)
            Command.ExistingImages = ExistingImages;
            Command.ImagesToDelete = ImagesToDelete;

            await _mediator.Send(Command);

            return RedirectToPage("/Properties/Index");
        }
    }
}