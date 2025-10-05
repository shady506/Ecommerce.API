namespace Ecommerce.API.DTOs.Response
{
    public class ProductWithRelatedRespnse
    {
        public Product Product { get; set; } = null!;
        public List<Product> RelatedProducts { get; set; } = null!;
        public List<Product> TopTraffic { get; set; } = null!;
        public List<Product> SimilarProducts { get; set; } = null!;
    }
}
