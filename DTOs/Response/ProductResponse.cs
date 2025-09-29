namespace Ecommerce.API.DTOs.Response
{
    public class ProductResponse
    {
            public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MainImg { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int Traffic { get; set; }
        public int Rate { get; set; }
        public double Discount { get; set; }
      

        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
    }
}
