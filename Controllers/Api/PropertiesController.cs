using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyCommissionService _propertyCommissionService;

        public PropertiesController(IPropertyCommissionService propertyCommissionService)
        {
            _propertyCommissionService = propertyCommissionService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProperties([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Ok(new List<object>());
            }

            try
            {
                var properties = await _propertyCommissionService.SearchPropertiesAsync(term, 10);
                
                var result = properties.Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    location = p.Location,
                    price = p.Price,
                    ownerName = !string.IsNullOrEmpty(p.User?.FirstName) && !string.IsNullOrEmpty(p.User?.LastName) 
                        ? $"{p.User.FirstName} {p.User.LastName}" 
                        : p.User?.Email ?? "Unknown"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Search failed", message = ex.Message });
            }
        }
    }
}