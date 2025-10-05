using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Ecommerce.API.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Order> _orderRepository;

        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartRepository, IRepository<Promotion> promotionRepository,
            IRepository<Order> orderRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _orderRepository = orderRepository;
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(CartRequest cartRequest)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == cartRequest.ProductId);

            if (cart is not null)
            {
                cart.Count += cartRequest.Count;
            }
            else
            {
                await _cartRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    ProductId = cartRequest.ProductId,
                    Count = cartRequest.Count
                });
            }

            await _cartRepository.CommitAsync();


            return Ok(new
            {
                msg = "Add Product To Cart Successfully"
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? code = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

            var totalPrice = carts.Sum(e => e.Product.Price * e.Count);

            string msg = "";
            if (code is not null)
            {
                var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code);
                if (promotion is null || !promotion.Status || DateTime.UtcNow > promotion.ValidTo)
                {
                    msg = "Invalid Code OR Expired";
                }
                else
                {
                    promotion.TotalUsed += 1;
                    await _promotionRepository.CommitAsync();

                    totalPrice = totalPrice - (totalPrice * 0.05);
                    msg = "Apply Promotion";
                }
            }



            return Ok(new 
            {
                carts,
                totalPrice,
                msg
            });
        }

        [HttpPatch("IncrementCart/{id}")]
        public async Task<IActionResult> IncrementCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);

            if (cart is null)
                return NotFound();

            cart.Count += 1;
            await _cartRepository.CommitAsync();

            return NoContent();
        }

        [HttpPatch("DecrementCart/{id}")]
        public async Task<IActionResult> DecrementCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);

            if (cart is null)
                return NotFound();

            if (cart.Count > 1)
            {
                cart.Count -= 1;
                await _cartRepository.CommitAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);

            if (cart is null)
                return NotFound();

            _cartRepository.Delete(cart);
            await _cartRepository.CommitAsync();

            return NoContent();
        }

        [HttpGet("Pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

            Order order = new()
            {
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                TransactionStatus = TransactionStatus.Pending,
                TotalPrice = (decimal)carts.Sum(e => e.Product.Price * e.Count),
                TransactionType =TransactionType.Visa,
                ApplicationUserId = user.Id
            };
            await _orderRepository.CreateAsync(order);
            await _orderRepository.CommitAsync();
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/api/Customer/Checkouts/Success/{order.OrderId}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/api/Customer/checkouts/cancel",
            };

            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)item.Product.Price * 100,
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);

            order.SessionId = session.Id;
            await _orderRepository.CommitAsync();

            return Ok(new
            {
                url = session.Url
            });
        }
    }
}
