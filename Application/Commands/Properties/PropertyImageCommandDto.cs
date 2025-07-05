namespace SteadyGrowth.Web.Application.Commands.Properties
{
    public class PropertyImageCommandDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string? Caption { get; set; }
        public string? ImageType { get; set; }
        public int DisplayOrder { get; set; }
    }
}
