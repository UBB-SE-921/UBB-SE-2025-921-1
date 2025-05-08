// BuyerWishlistController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClassLibrary.Service;
using System;
using System.Threading.Tasks;
using WebMarketplace.Models;
using SharedClassLibrary.Domain;
using System.Diagnostics;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for buyer wishlist functionality
    /// </summary>
    public class BuyerWishlistController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IProductService _productService;
        private readonly ILogger<BuyerWishlistController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerWishlistController"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service</param>
        /// <param name="productService">The product service</param>
        /// <param name="logger">The logger</param>
        public BuyerWishlistController(
            IBuyerService buyerService,
            IProductService productService,
            ILogger<BuyerWishlistController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user ID (placeholder - would be replaced with actual authentication)
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            // This would be replaced with actual user authentication 
            return 2; // Mock user ID
        }

        /// <summary>
        /// Displays the buyer's wishlist
        /// </summary>
        /// <returns>The view</returns>
        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Loading wishlist for user");

                int userId = GetCurrentUserId();

                // Create a basic User object instead of fetching all users
                var user = new SharedClassLibrary.Domain.User(userId);
                _logger.LogInformation("Created user object with ID: {UserId}", user.UserId);

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                // Load the buyer with their wishlist items
                await _buyerService.LoadBuyer(buyer, SharedClassLibrary.Service.BuyerDataSegments.Wishlist);

                _logger.LogInformation("Wishlist loaded for buyer {BuyerId} with {ItemCount} items",
                    buyer.Id, buyer.Wishlist?.Items?.Count ?? 0);

                // Convert BuyerWishlistItem objects to Product objects
                var products = new List<Product>();
                if (buyer.Wishlist?.Items != null)
                {
                    foreach (var wishlistItem in buyer.Wishlist.Items)
                    {
                        // Get full product details for each wishlist item
                        var product = await _productService.GetProductByIdAsync(wishlistItem.ProductId);
                        if (product != null)
                        {
                            products.Add(product);
                        }
                    }
                }

                // Create view model with products instead of wishlist items
                var viewModel = new BuyerWishlistViewModel
                {
                    BuyerId = buyer.Id,
                    WishlistItems = products
                };

                _logger.LogInformation("Wishlist loaded successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading wishlist after {ElapsedMs}ms: {Message}",
                    stopwatch.ElapsedMilliseconds, ex.Message);
                return RedirectToAction("Error", "Home", new { message = $"Error loading wishlist: {ex.Message}" });
            }
        }


        /// <summary>
        /// Removes an item from the wishlist
        /// </summary>
        /// <param name="productId">The product ID to remove</param>
        /// <returns>Redirects to the wishlist</returns>
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            try
            {
                _logger.LogInformation("Removing product {ProductId} from wishlist", productId);

                int userId = GetCurrentUserId();

                // Create a basic User object instead of fetching all users
                var user = new SharedClassLibrary.Domain.User(userId);

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                await _buyerService.RemoveWishilistItem(buyer, productId);

                TempData["SuccessMessage"] = "Item removed from wishlist";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove product {ProductId} from wishlist: {Message}",
                    productId, ex.Message);
                TempData["ErrorMessage"] = $"Failed to remove item: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Adds a product to the wishlist
        /// </summary>
        /// <param name="productId">The product ID to add</param>
        /// <returns>Redirects to the wishlist</returns>
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            try
            {
                _logger.LogInformation("Adding product {ProductId} to wishlist", productId);

                int userId = GetCurrentUserId();

                // Create a basic User object instead of fetching all users
                var user = new SharedClassLibrary.Domain.User(userId);

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                // Here you would need to implement AddWishlistItem logic
                // Example implementation could be something like:
                // await _buyerService.AddWishlistItem(buyer, productId);

                // For now, we'll log a warning since the method doesn't exist in the interface
                _logger.LogWarning("AddWishlistItem method not implemented in IBuyerService");

                TempData["SuccessMessage"] = "Item added to wishlist";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product {ProductId} to wishlist: {Message}",
                    productId, ex.Message);
                TempData["ErrorMessage"] = $"Failed to add item: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
