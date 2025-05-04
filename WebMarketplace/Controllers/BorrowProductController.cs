using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebMarketplace.Models;
using SharedClassLibrary.Service.Web;
using SharedClassLibrary.Domain;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WebIProductService = SharedClassLibrary.Service.Web.IWebProductService;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for the borrow product functionality
    /// </summary>
    public class BorrowProductController : Controller
    {
        private readonly IWaitlistService _waitlistService;
        private readonly WebIProductService _productService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<BorrowProductController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BorrowProductController"/> class.
        /// </summary>
        /// <param name="waitlistService">The waitlist service</param>
        /// <param name="productService">The product service</param>
        /// <param name="notificationService">The notification service</param>
        /// <param name="logger">The logger</param>
        public BorrowProductController(
            IWaitlistService waitlistService,
            WebIProductService productService,
            INotificationService notificationService,
            ILogger<BorrowProductController> logger)
        {
            _waitlistService = waitlistService ?? throw new ArgumentNullException(nameof(waitlistService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user ID (placeholder - would be replaced with actual authentication)
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            // This would be replaced with actual user authentication 
            return 1;
        }

        /// <summary>
        /// Displays the borrow product page
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>The view</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get product with ID: {ProductId}", id);
                
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Product {ProductName} (ID: {ProductId}) found", product.Name, id);
                
                string sellerName = await _productService.GetSellerNameAsync(product.SellerId);
                int currentUserId = GetCurrentUserId();
                bool isOnWaitlist = await _waitlistService.IsUserInWaitlist(currentUserId, id);
                int position = isOnWaitlist ? await _waitlistService.GetUserWaitlistPosition(currentUserId, id) : 0;
                int unreadNotificationsCount = await _notificationService.GetUnreadNotificationsCountAsync(currentUserId);

                // Create the view model with proper type conversions
                var viewModel = new BorrowProductViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price, // Now both are double
                    SellerId = product.SellerId, // Both are int, no need to cast
                    SellerName = sellerName,
                    ProductType = product.ProductType,
                    // DateTimeOffset will be properly handled by our property setters
                    StartDate = product.StartDate,
                    EndDate = product.EndDate,
                    IsOnWaitlist = isOnWaitlist,
                    WaitlistPosition = position,
                    UnreadNotificationsCount = unreadNotificationsCount
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error loading product with ID {ProductId}", id);
                return RedirectToAction("Error", "Home", new { message = $"Error loading product: {ex.Message}" });
            }
        }

        /// <summary>
        /// Shows an index of available products
        /// </summary>
        /// <returns>The index view</returns>
        public IActionResult Index()
        {
            // Provide some guidance to the user about available products
            return View();
        }

        /// <summary>
        /// Joins the waitlist for a product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>Redirects to the product details</returns>
        [HttpPost]
        public async Task<IActionResult> JoinWaitlist(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                await _waitlistService.AddUserToWaitlist(currentUserId, id);
                
                TempData["SuccessMessage"] = "You've joined the waitlist!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join waitlist for product {ProductId}", id);
                TempData["ErrorMessage"] = $"Failed to join waitlist: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Leaves the waitlist for a product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>Redirects to the product details</returns>
        [HttpPost]
        public async Task<IActionResult> LeaveWaitlist(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                await _waitlistService.RemoveUserFromWaitlist(currentUserId, id);
                
                TempData["SuccessMessage"] = "You've left the waitlist";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to leave waitlist for product {ProductId}", id);
                TempData["ErrorMessage"] = $"Failed to leave waitlist: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Shows the user's position in the waitlist
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>JSON with the position information</returns>
        [HttpGet]
        public async Task<IActionResult> GetWaitlistPosition(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                int position = await _waitlistService.GetUserWaitlistPosition(currentUserId, id);
                
                return Json(new { position });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get waitlist position for product {ProductId}", id);
                return Json(new { error = $"Failed to get waitlist position: {ex.Message}" });
            }
        }

        /// <summary>
        /// Shows the notifications page
        /// </summary>
        /// <returns>The notifications partial view</returns>
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                var notificationService = HttpContext.RequestServices.GetRequiredService<SharedClassLibrary.Service.Web.INotificationService>();
                var notifications = await notificationService.GetUserNotificationsAsync(currentUserId);
                
                return PartialView("_Notifications", notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications");
                return Json(new { error = $"Couldn't load notifications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets notifications in JSON format for direct testing
        /// </summary>
        /// <returns>JSON notifications data</returns>
        [HttpGet]
        public async Task<IActionResult> NotificationsJson()
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                var notificationService = HttpContext.RequestServices.GetRequiredService<SharedClassLibrary.Service.Web.INotificationService>();
                var notifications = await notificationService.GetUserNotificationsAsync(currentUserId);
                
                return Json(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications JSON");
                return Json(new { error = $"Couldn't load notifications: {ex.Message}" });
            }
        }
    }
} 