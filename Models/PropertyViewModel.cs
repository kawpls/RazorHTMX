namespace RazorHTMX.Models
{
    public class PropertyViewModel
    {
        public required string Title { get; set; }
        public required string Price { get; set; }
        public required string ImageUrl { get; set; }
        public int Inquiries { get; set; }
    }
}
