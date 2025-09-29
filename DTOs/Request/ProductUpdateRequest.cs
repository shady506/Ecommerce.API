namespace Ecommerce.API.DTOs.Request
{
    public class ProductUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? MainImg { get; set; } 
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public bool Status { get; set; }
    }
}
