using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IPropertyService _propertyService;

        public List<PropertyDto> FeaturedProperties { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger, IPropertyService propertyService)
        {
            _logger = logger;
            _propertyService = propertyService;
        }

        public async Task OnGetAsync()
        {
            var properties = await _propertyService.GetApprovedPropertiesAsync(1, 12);
            FeaturedProperties = properties.Select(p => (PropertyDto)p).ToList();
        }
    }
}
