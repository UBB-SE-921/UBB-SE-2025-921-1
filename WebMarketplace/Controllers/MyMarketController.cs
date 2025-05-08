using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Service;
using SharedClassLibrary.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebMarketplace.Controllers
{
    public class MyMarketController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IShoppingCartService _shoppingCartService;

        public MyMarketController(IBuyerService buyerService, IShoppingCartService shoppingCartService)
        {
            _buyerService = buyerService;
            _shoppingCartService = shoppingCartService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var buyerId = GetBuyerId();
            var followedSellers = await _buyerService.GetFollowingUsersIDs(buyerId);
            var products = await _buyerService.GetProductsFromFollowedSellers(followedSellers);

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var buyerId = GetBuyerId();
            await _shoppingCartService.AddProductToCartAsync(buyerId, productId);
            return Json(new { success = true });
        }

        private int GetBuyerId()
        {
            // Replace with actual logic to retrieve the current buyer's ID
            return UserSession.CurrentUserId ?? 0;
        }
    }
}
