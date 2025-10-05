namespace Ecommerce.API.Models
{
    public enum TransactionType
    {
        Visa,
        Cash
        
    }

    public enum OrderStatus
    {
        Pending,
        UnShipped,
        Shipped,
        Completed,
        Canceled


    }
    public enum TransactionStatus
    {
        Pending,
        Confirmed,

    }
    public class Order
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SessionId { get; set; }
        public string? TransactionId { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus TransactionStatus { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public DateTime OrderDate{ get; set; }
        public string? CarrierId { get; set; }
        public string? CarrierName { get; set; }
        public DateTime ShippedDate{ get; set; }

        public string? ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
