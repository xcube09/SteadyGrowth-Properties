using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Implementations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties
{
    [Authorize(Roles = "Admin")]
    public class AddModel : PageModel
    {
        private readonly PropertyService _propertyService;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;

        public AddModel(PropertyService propertyService, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _propertyService = propertyService;
            _userManager = userManager;
            _env = env;
            Property = new Property();
            Images = new List<IFormFile>();
        }

        [BindProperty]
        public Property Property { get; set; }

        [BindProperty]
        [Display(Name = "Property Images")]
        public List<IFormFile> Images { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            Property.Status = PropertyStatus.Approved;
            Property.UserId = user.Id;
            Property.CreatedAt = DateTime.UtcNow;

            var created = await _propertyService.CreatePropertyAsync(Property, user.Id);

            if (Images != null && Images.Any())
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "properties");
                Directory.CreateDirectory(uploadPath);
                int order = 0;

                foreach (var file in Images)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var propertyImage = new PropertyImage
                        {
                            PropertyId = created.Id,
                            FileName = fileName,
                            DisplayOrder = order++,
                            UploadedAt = DateTime.UtcNow
                        };

                        if (created.PropertyImages == null)
                            created.PropertyImages = new List<PropertyImage>();

                        created.PropertyImages.Add(propertyImage);
                    }
                }

                await _propertyService.UpdatePropertyAsync(created);
            }

            return RedirectToPage("/Properties/Index");
        }
    }
}
