using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Application.ViewModels
{
    public class PropertyImageUploadModel
    {
        [Required(ErrorMessage = "Image file is required.")]
        public IFormFile? ImageFile { get; set; }

        [StringLength(500)]
        public string? Caption { get; set; }

        [StringLength(100)]
        public string? ImageType { get; set; }

        public int DisplayOrder { get; set; }
    }
}
