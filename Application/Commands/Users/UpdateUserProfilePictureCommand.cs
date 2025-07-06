using MediatR;
using Microsoft.AspNetCore.Http;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SteadyGrowth.Web.Application.Commands.Users
{
    public class UpdateUserProfilePictureCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public IFormFile ProfilePicture { get; set; }

        public class UpdateUserProfilePictureCommandHandler : IRequestHandler<UpdateUserProfilePictureCommand, bool>
        {
            private readonly ApplicationDbContext _context;
            private readonly IWebHostEnvironment _webHostEnvironment;

            public UpdateUserProfilePictureCommandHandler(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
            {
                _context = context;
                _webHostEnvironment = webHostEnvironment;
            }

            public async Task<bool> Handle(UpdateUserProfilePictureCommand request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    return false;
                }

                if (request.ProfilePicture != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profilepictures");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, user.ProfilePictureUrl);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ProfilePicture.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ProfilePicture.CopyToAsync(fileStream);
                    }
                    user.ProfilePictureUrl = uniqueFileName;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }
}
