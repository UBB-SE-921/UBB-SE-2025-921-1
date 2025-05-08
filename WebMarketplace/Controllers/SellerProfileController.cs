using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Service;
using SharedClassLibrary.Domain;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class SellerProfileController : Controller
    {
        private readonly ISellerService _sellerService;

        public SellerProfileController(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int sellerId)
        {
            var seller = await _sellerService.GetSellerByIdAsync(sellerId);
            var products = await _sellerService.GetAllProducts(sellerId);

            var viewModel = new SellerProfileViewModel
            {
                Seller = seller,
                Products = products
            };

            return View(viewModel);
        }
    }
}
