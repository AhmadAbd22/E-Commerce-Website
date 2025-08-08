using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceWebsite.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IOrderServiceRepository _orderService;
        private readonly IOrderRepository _orderRepo;

        public CartController(
            ICartRepository cartRepo,
            IBookRepository bookRepo,
            IOrderServiceRepository orderService,
            IOrderRepository orderRepo) 
        {
            _cartRepo = cartRepo;
            _bookRepo = bookRepo;
            _orderService = orderService;
            _orderRepo = orderRepo; 
        }

        private Guid GetCurrentUserId()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("UserId")
                               ?? User.FindFirst("Id")
                               ?? User.FindFirst(ClaimTypes.Sid);

                if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                {
                    throw new UnauthorizedAccessException("User ID claim not found in authentication token");
                }

                if (Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return userId;
                }

                throw new UnauthorizedAccessException("Invalid user ID format in authentication token");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Error retrieving user ID from authentication context", ex);
            }
        }

        private (Guid UserId, string Username, string Role) GetCurrentUserInfo()
        {
            var userId = GetCurrentUserId();
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            return (userId, username, role);
        }

        private bool IsCurrentUserCustomer()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid bookId, int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (!IsCurrentUserCustomer())
                {
                    TempData["Error"] = "Admin users cannot add items to cart.";
                    return RedirectToAction("UserHome", "UserHome");
                }

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
            catch (UnauthorizedAccessException ex)
            {
                TempData["Error"] = "Please log in to add items to your cart.";
                return RedirectToAction("Login", "Login");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("UserHome", "UserHome");
            }
            catch (Exception ex)
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
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to view your cart.";
                return RedirectToAction("Login", "Login");
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
                var userId = GetCurrentUserId();

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

                if (cartItem.UserId != userId)
                {
                    TempData["Error"] = "Unauthorized access to cart item.";
                    return RedirectToAction("ViewCart");
                }

                cartItem.Quantity = quantity;
                await _cartRepo.UpdateCartItemAsync(cartItem);

                TempData["Success"] = "Cart updated successfully!";
                return RedirectToAction("ViewCart");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to update your cart.";
                return RedirectToAction("Login", "Login");
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
                var userId = GetCurrentUserId();

                var cartItem = await _cartRepo.GetCartItemByIdAsync(cartItemId);
                if (cartItem == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("ViewCart");
                }

                if (cartItem.UserId != userId)
                {
                    TempData["Error"] = "Unauthorized access to cart item.";
                    return RedirectToAction("ViewCart");
                }

                await _cartRepo.RemoveCartItemAsync(cartItemId);
                TempData["Success"] = "Item removed from cart.";
                return RedirectToAction("ViewCart");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to modify your cart.";
                return RedirectToAction("Login", "Login");
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
                        Subtotal = (ci.Book?.Price ?? 0) * ci.Quantity,
                        ImageUrl = ci.Book?.ImageUrl ?? "",
                        AuthorName = ci.Book?.Author?.AuthorName ?? "Unknown"
                    }).ToList(),
                    TotalAmount = total
                };

                return View(checkoutDto);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to checkout.";
                return RedirectToAction("Login", "Login");
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
                var userId = GetCurrentUserId();
                var cartItems = await _cartRepo.GetCartItemsByUserIdAsync(userId);
                var total = await _orderService.CalculateCartTotalAsync(userId);

                checkoutDto.CartItems = cartItems.Select(ci => new CartItemDto
                {
                    BookTitle = ci.Book?.Title ?? "Unknown",
                    BookPrice = ci.Book?.Price ?? 0,
                    Quantity = ci.Quantity,
                    Subtotal = (ci.Book?.Price ?? 0) * ci.Quantity,
                    ImageUrl = ci.Book?.ImageUrl ?? "",
                    AuthorName = ci.Book?.Author?.AuthorName ?? "Unknown"
                }).ToList();
                checkoutDto.TotalAmount = total;

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
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to place an order.";
                return RedirectToAction("Login", "Login");
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
            try
            {
                var userId = GetCurrentUserId();

                var order = await _orderRepo.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("ViewCart");
                }

                if (order.UserId != userId)
                {
                    TempData["Error"] = "Unauthorized access to order.";
                    return RedirectToAction("UserHome", "UserHome");
                }

                var orderConfirmationDto = new OrderConfirmationDto
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    City = order.City,
                    PhoneNumber = order.PhoneNumber,
                    EstimatedDeliveryDate = order.OrderDate.AddDays(5),
                    OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
                    {
                        BookTitle = oi.Book?.Title ?? "Unknown Book",
                        AuthorName = oi.Book?.Author?.AuthorName ?? "Unknown Author",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.Quantity * oi.UnitPrice,
                        ImageUrl = oi.Book?.ImageUrl ?? ""
                    }).ToList() ?? new List<OrderItemDto>()
                };

                return View(orderConfirmationDto);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to view order confirmation.";
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Order placement error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading order confirmation.";
                return RedirectToAction("UserHome", "UserHome");
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderHistory()
        {
            try
            {
                var userId = GetCurrentUserId();

                var orders = await _orderRepo.GetOrderByUserIdAsync(userId);

                if (!orders.Any())
                {
                    var emptyDto = new OrderHistoryDto
                    {
                        Orders = new List<OrderHistoryItemDto>(),
                        TotalOrders = 0,
                        TotalSpent = 0
                    };
                    return View(emptyDto);
                }

                var orderHistoryItems = orders.Select(order => new OrderHistoryItemDto
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    TotalItems = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0,
                    ShippingAddress = order.ShippingAddress,
                    City = order.City,
                    PhoneNumber = order.PhoneNumber,
                    EstimatedDeliveryDate = GetEstimatedDeliveryDate(order.OrderDate, order.OrderStatus),
                    StatusColor = GetStatusColor(order.OrderStatus),
                    StatusIcon = GetStatusIcon(order.OrderStatus),
                    OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
                    {
                        BookId = oi.BookId ?? Guid.Empty,
                        BookTitle = oi.Book?.Title ?? "Unknown Book",
                        AuthorName = oi.Book?.Author?.AuthorName ?? "Unknown Author",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.Quantity * oi.UnitPrice,
                        ImageUrl = oi.Book?.ImageUrl ?? "",
                        PriceAtPurchase = oi.UnitPrice
                    }).ToList() ?? new List<OrderItemDto>()
                }).OrderByDescending(o => o.OrderDate).ToList();

                var orderHistoryDto = new OrderHistoryDto
                {
                    Orders = orderHistoryItems,
                    TotalOrders = orders.Count,
                    TotalSpent = orders.Sum(o => o.TotalAmount)
                };

                return View(orderHistoryDto);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Please log in to view your order history.";
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Order history error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading your order history.";
                return RedirectToAction("UserHome", "UserHome");
            }
        }

        private static DateTime? GetEstimatedDeliveryDate(DateTime orderDate, string status)
        {
            return status.ToLower() switch
            {
                "pending" => orderDate.AddDays(7),
                "confirmed" => orderDate.AddDays(5),
                "shipped" => orderDate.AddDays(3),
                "delivered" => null,
                "cancelled" => null,
                _ => orderDate.AddDays(7)
            };
        }

        private static string GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "warning",
                "confirmed" => "info",
                "shipped" => "primary",
                "delivered" => "success",
                "cancelled" => "danger",
                _ => "default"
            };
        }

        private static string GetStatusIcon(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "icon-hour-glass2",
                "confirmed" => "icon-checkmark3",
                "shipped" => "icon-truck",
                "delivered" => "icon-check",
                "cancelled" => "icon-cross2",
                _ => "icon-info22"
            };
        }
    }
}

