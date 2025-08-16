using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
	public enum PropertyType
	{
		Residential = 0,
		Commercial = 1,
		Land = 2,
		Mixed = 3
	}

	public enum PropertyStatus
	{
		Draft = 0,
		Pending = 1,
		Approved = 2,
		Rejected = 3
	}

	/// <summary>
	/// Property entity for real estate listings
	/// </summary>
	public class Property
	{
		public int Id { get; set; }
		
		[Required, StringLength(200)]
		public string Title { get; set; } = string.Empty;
		
		[Required, StringLength(2000)]
		public string Description { get; set; } = string.Empty;
		
		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }

		[StringLength(3)]
		public string? CurrencyCode { get; set; }
		
		[Required, StringLength(500)]
		public string Location { get; set; } = string.Empty;
		
		public PropertyType PropertyType { get; set; }
		
		public PropertyStatus Status { get; set; } = PropertyStatus.Draft;
		
		[Required]
		public string UserId { get; set; } = string.Empty;
		
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		
		public DateTime? ApprovedAt { get; set; }
		
		public string? Images { get; set; } // JSON array
		
		public string? Features { get; set; } // JSON array
		
		public bool IsActive { get; set; } = true;
		
		// Navigation properties
		public virtual User User { get; set; } = null!;
		public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
	}
}
