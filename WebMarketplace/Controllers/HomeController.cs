using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Mock data sources
        private static List<BuyerFamilySyncItemViewModel> _buyerFamilySyncItems = new List<BuyerFamilySyncItemViewModel>
        {
            new BuyerFamilySyncItemViewModel { Id = 1, DisplayName = "John Doe", Status = "Confirmed" },
            new BuyerFamilySyncItemViewModel { Id = 2, DisplayName = "Jane Smith", Status = "Pending" }
        };

        private static List<BuyerFamilySyncViewModel> _buyerFamilySyncs = new List<BuyerFamilySyncViewModel>
        {
            new BuyerFamilySyncViewModel { Id = 1, LinkedBuyerName = "Alice Johnson", Status = "Possible" }
        };

        private static List<BuyerBadgeViewModel> _buyerBadges = new List<BuyerBadgeViewModel>
        {
            new BuyerBadgeViewModel { Id = 1, BadgeName = "Gold", Discount = 15, Progress = 80, ImageSource = "/images/gold.png" }
        };

        private static List<BuyerAddressViewModel> _buyerAddresses = new List<BuyerAddressViewModel>
        {
            new BuyerAddressViewModel { Id = 1, StreetLine = "123 Main St", City = "New York", Country = "USA", PostalCode = "10001" }
        };

        private static List<BuyerAddressViewModel> _buyerShippingAddresses = new List<BuyerAddressViewModel>
        {
            new BuyerAddressViewModel { Id = 2, StreetLine = "456 Elm St", City = "Los Angeles", Country = "USA", PostalCode = "90001" }
        };

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Combine all data into a single view model
            var viewModel = new HomeViewModel
            {
                BuyerFamilySyncItems = _buyerFamilySyncItems,
                BuyerFamilySyncs = _buyerFamilySyncs,
                BuyerBadges = _buyerBadges,
                BuyerAddresses = _buyerAddresses,
                BuyerShippingAddresses = _buyerShippingAddresses
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult EditAddress()
        {
            // Load the current address data (mock data for now)
            var address = _buyerAddresses.FirstOrDefault();
            return PartialView("_EditAddress", address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAddress(BuyerAddressViewModel address)
        {
            if (ModelState.IsValid)
            {
                // Save the modified address (mock saving for now)
                var existingAddress = _buyerAddresses.FirstOrDefault(a => a.Id == address.Id);
                if (existingAddress != null)
                {
                    existingAddress.StreetLine = address.StreetLine;
                    existingAddress.City = address.City;
                    existingAddress.Country = address.Country;
                    existingAddress.PostalCode = address.PostalCode;
                }
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_EditAddress", address);
        }

        public IActionResult EditShippingAddress()
        {
            // Load the current shipping address data (mock data for now)
            var shippingAddress = _buyerShippingAddresses.FirstOrDefault();
            return PartialView("_EditShippingAddress", shippingAddress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveShippingAddress(BuyerAddressViewModel shippingAddress)
        {
            if (ModelState.IsValid)
            {
                // Save the modified shipping address (mock saving for now)
                var existingShippingAddress = _buyerShippingAddresses.FirstOrDefault(a => a.Id == shippingAddress.Id);
                if (existingShippingAddress != null)
                {
                    existingShippingAddress.StreetLine = shippingAddress.StreetLine;
                    existingShippingAddress.City = shippingAddress.City;
                    existingShippingAddress.Country = shippingAddress.Country;
                    existingShippingAddress.PostalCode = shippingAddress.PostalCode;
                }
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_EditShippingAddress", shippingAddress);
        }
    }
}
