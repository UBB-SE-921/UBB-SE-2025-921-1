using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Service;
using SharedClassLibrary.Domain;
using System.Threading.Tasks;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class MyMarketProfileController : Controller
    {
        private readonly IBuyerService _buyerService;

        public MyMarketProfileController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int sellerId)
        {
            var seller = await _buyerService.GetSellerByIdAsync(sellerId);
            var products = await _buyerService.GetProductsForViewProfile(sellerId);

            var viewModel = new MyMarketProfileViewModel
            {
                Seller = seller,
                Products = products
            };

            return View(viewModel);
        }
    }
}
