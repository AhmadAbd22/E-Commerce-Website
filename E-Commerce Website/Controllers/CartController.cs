using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebsite.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IOrderService _orderService;

        public CartController(ICartRepository cartRepo, IBookRepository bookRepo, IOrderService orderService)
        {
            _cartRepo = cartRepo;
            _bookRepo = bookRepo;
            _orderService = orderService;
        }

        // TODO: Replace with actual user authentication
        private Guid GetCurrentUserId()
        {
            // For now, return a hardcoded user ID
            // In production, get this from authentication/session
            return new Guid("11111111-1111-1111-1111-111111111111");
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid bookId, int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                var book = await _bookRepo.GetActiveBookByIdAsync(bookId);
                
                if (book == null)
                {
                    TempData["Error"] = "Book not found!";
                    return RedirectToAction("UserHome", "UserHome");
                }

                if (book.StockQuantity < quantity)
                {
                    TempData["Error"] = "Not enough stock available!";
                    return RedirectToAction("UserHome", "UserHome");
                }

                var cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = bookId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _cartRepo.AddToCartAsync(cartItem);
                TempData["Success"] = $"{book.Title} has been added to your cart!";
                
                return RedirectToAction("UserHome", "UserHome");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("UserHome", "UserHome");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while adding to cart.";
                return RedirectToAction("UserHome", "UserHome");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItems = await _cartRepo.GetCartItemsByUserIdAsync(userId);
                var cartTotal = await _cartRepo.GetCartTotalAsync(userId);

                var cartDto = new CartDto
                {
                    CartItems = cartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        BookId = ci.BookId ?? Guid.Empty,
                        BookTitle = ci.Book?.Title ?? "Unknown",
                        BookPrice = ci.Book?.Price ?? 0,
                        Quantity = ci.Quantity,
                        Subtotal = (ci.Book?.Price ?? 0) * ci.Quantity,
                        ImageUrl = ci.Book?.ImageUrl ?? "",
                        AuthorName = ci.Book?.Author?.AuthorName ?? "Unknown",
                        StockAvailable = ci.Book?.StockQuantity ?? 0
                    }).ToList(),
                    TotalAmount = cartTotal,
                    ItemCount = cartItems.Sum(ci => ci.Quantity)
                };

                return View(cartDto);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading cart.";
                return RedirectToAction("UserHome", "UserHome");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid cartItemId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCart(cartItemId);
                }

                var cartItem = await _cartRepo.GetCartItemByIdAsync(cartItemId);
                if (cartItem == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("ViewCart");
                }

                cartItem.Quantity = quantity;
                await _cartRepo.UpdateCartItemAsync(cartItem);
                
                TempData["Success"] = "Cart updated successfully!";
                return RedirectToAction("ViewCart");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("ViewCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            try
            {
                await _cartRepo.RemoveCartItemAsync(cartItemId);
                TempData["Success"] = "Item removed from cart.";
                return RedirectToAction("ViewCart");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error removing item from cart.";
                return RedirectToAction("ViewCart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItems = await _cartRepo.GetCartItemsByUserIdAsync(userId);
                
                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("ViewCart");
                }

                // Validate stock before checkout
                if (!await _orderService.ValidateCartStockAsync(userId))
                {
                    TempData["Error"] = "Some items in your cart are no longer available in the requested quantity.";
                    return RedirectToAction("ViewCart");
                }

                var total = await _orderService.CalculateCartTotalAsync(userId);
                
                var checkoutDto = new CheckoutDto
                {
                    CartItems = cartItems.Select(ci => new CartItemDto
                    {
                        BookTitle = ci.Book?.Title ?? "Unknown",
                        BookPrice = ci.Book?.Price ?? 0,
                        Quantity = ci.Quantity,
                        Subtotal = (ci.Book?.Price ?? 0) * ci.Quantity
                    }).ToList(),
                    TotalAmount = total
                };

                return View(checkoutDto);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading checkout page.";
                return RedirectToAction("ViewCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutDto checkoutDto)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", checkoutDto);
            }

            try
            {
                var userId = GetCurrentUserId();
                
                var order = await _orderService.PlaceOrderFromCartAsync(
                    userId,
                    checkoutDto.ShippingAddress,
                    checkoutDto.City,
                    checkoutDto.PhoneNumber
                );

                TempData["Success"] = $"Order placed successfully! Order ID: {order.Id}";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return View("Checkout", checkoutDto);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while placing your order.";
                return View("Checkout", checkoutDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(Guid orderId)
        {
            // Implementation for order confirmation page
            return View();
        }
    }
}
