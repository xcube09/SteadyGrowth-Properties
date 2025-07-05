using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Properties;
using SteadyGrowth.Web.Application.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using SteadyGrowth.Web.Models.Entities;
using System;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties
{
    public class AddModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;

        public AddModel(IMediator mediator, IWebHostEnvironment webHostEnvironment, UserManager<User> userManager)
        {
            _mediator = mediator;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [BindProperty]
        public AddPropertyCommand? Command { get; set; }

        [BindProperty]
        public List<PropertyImageUploadModel> Images { get; set; } = new List<PropertyImageUploadModel>();

        public void OnGet()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("My Properties", "/Membership/Properties/Index") };
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

            foreach (var image in Images)
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

                    Command.Images.Add(new PropertyImageCommandDto
                    {
                        FileName = uniqueFileName,
                        Caption = image.Caption,
                        ImageType = image.ImageType,
                        DisplayOrder = image.DisplayOrder
                    });
                }
            }

            var property = await _mediator.Send(Command);

            return RedirectToPage("/Properties/Index");
        }
    }
}
