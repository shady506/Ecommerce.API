using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace Ecommerce.API.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CheckoutsController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutsController> _logger;

        public CheckoutsController(IRepository<Order> orderRepository,UserManager<ApplicationUser> userManager,
            IRepository<Cart> cartRepository,IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,ApplicationDbContext context,ILogger<CheckoutsController> logger)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _context = context;
            _logger = logger;
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> Success(int orderId)
        {
          var transaction = _context.Database.BeginTransaction();

            try
            {
                var order = await _orderRepository.GetOneAsync(e => e.OrderId == orderId);
                if (order == null)
                    return NotFound();
                var service = new SessionService();
                var session = service.Get(order.SessionId);


                order.TransactionId = session.PaymentIntentId;
                order.TransactionStatus = TransactionStatus.Confirmed;
                order.OrderStatus = OrderStatus.UnShipped;

               await _orderItemRepository.CommitAsync();

                var user = await _userManager.GetUserAsync(User);

                if (user is null)
                    return NotFound();


                var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

                var orderItems = carts.Select(e => new OrderItem
                {
                    ProductId = e.ProductId,
                    Count = e.Count,
                    OrderId = orderId,
                    TotalPrice = (decimal)carts.Sum(e => e.Product.Price * e.Count)
                }).ToList();

                await _orderItemRepository.AddRangeAsync(orderItems);
                await _orderItemRepository.CommitAsync();

                foreach (var item in carts)
                {
                    item.Product.Quantity -= item.Count;
                }
                await _productRepository.CommitAsync();

                foreach (var item in carts)
                {
                    _cartRepository.Delete(item);
                }

                await _cartRepository.CommitAsync();

                transaction.Commit();
                return Ok(new
                {
                    msg = "Place Order Successfully"
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex.Message);
                return BadRequest(new
                {
                    msg = "Filed Place Order"
                });

            }
        }

    }
}
