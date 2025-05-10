using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebMarketplace.Models;
using System.Security.Claims;
using SharedClassLibrary.Domain;
using Microsoft.Extensions.Logging;
using SharedClassLibrary.Service;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebMarketplace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBuyerAddressService _buyerAddressService;
        private readonly IBuyerService _buyerService;

        public HomeController(ILogger<HomeController> logger,
                             IBuyerAddressService buyerAddressService,
                             IBuyerService buyerService)
        {
            _logger = logger;
            _buyerAddressService = buyerAddressService;
            _buyerService = buyerService;
        }

        public async Task<IActionResult> Index()
        {
            // Get the current logged-in user's ID
            var userId = UserSession.CurrentUserId;
            if (userId == null)
            {
                return RedirectToAction("Index", "Login"); // Redirect to login if user is not authenticated
            }

            try
            {
                // Get the buyer info for the current user
                var user = new User { UserId = (int)userId };
                var buyer = await _buyerService.GetBuyerByUser(user);

                // Create the view model
                var viewModel = new HomeViewModel();

                // Get buyer addresses
                viewModel.BuyerAddresses = new List<BuyerAddressViewModel>();
                if (buyer.BillingAddress != null)
                {
                    viewModel.BuyerAddresses.Add(new BuyerAddressViewModel
                    {
                        Id = buyer.BillingAddress.Id,
                        StreetLine = buyer.BillingAddress.StreetLine,
                        City = buyer.BillingAddress.City,
                        Country = buyer.BillingAddress.Country,
                        PostalCode = buyer.BillingAddress.PostalCode
                    });
                }
                else
                {
                    viewModel.BuyerAddresses.Add(new BuyerAddressViewModel
                    {
                        Id = 0,
                        StreetLine = "",
                        City = "",
                        Country = "",
                        PostalCode = ""
                    });
                }

                // Get buyer shipping addresses
                viewModel.BuyerShippingAddresses = new List<BuyerAddressViewModel>();
                if (buyer.UseSameAddress)
                {
                    // If using same address, use the billing address as shipping
                    if (buyer.BillingAddress != null)
                    {
                        viewModel.BuyerShippingAddresses.Add(new BuyerAddressViewModel
                        {
                            Id = buyer.BillingAddress.Id,
                            StreetLine = buyer.BillingAddress.StreetLine,
                            City = buyer.BillingAddress.City,
                            Country = buyer.BillingAddress.Country,
                            PostalCode = buyer.BillingAddress.PostalCode
                        });
                    }
                }
                else if (buyer.ShippingAddress != null)
                {
                    viewModel.BuyerShippingAddresses.Add(new BuyerAddressViewModel
                    {
                        Id = buyer.ShippingAddress.Id,
                        StreetLine = buyer.ShippingAddress.StreetLine,
                        City = buyer.ShippingAddress.City,
                        Country = buyer.ShippingAddress.Country,
                        PostalCode = buyer.ShippingAddress.PostalCode
                    });
                }
                else
                {
                    viewModel.BuyerShippingAddresses.Add(new BuyerAddressViewModel
                    {
                        Id = 0,
                        StreetLine = "",
                        City = "",
                        Country = "",
                        PostalCode = ""
                    });
                }

                // Get buyer family sync items from linkages
                viewModel.BuyerFamilySyncItems = new List<BuyerFamilySyncItemViewModel>();
                if (buyer.Linkages != null)
                {
                    foreach (var linkage in buyer.Linkages)
                    {
                        viewModel.BuyerFamilySyncItems.Add(new BuyerFamilySyncItemViewModel
                        {
                            Id = linkage.Buyer.Id,
                            DisplayName = $"{linkage.Buyer.FirstName} {linkage.Buyer.LastName}",
                            Status = linkage.Status.ToString()
                        });
                    }
                }

                // For BuyerFamilySyncs - we'll use the same linkages data but formatted differently
                viewModel.BuyerFamilySyncs = new List<BuyerFamilySyncViewModel>();
                if (buyer.Linkages != null)
                {
                    foreach (var linkage in buyer.Linkages)
                    {
                        viewModel.BuyerFamilySyncs.Add(new BuyerFamilySyncViewModel
                        {
                            Id = linkage.Buyer.Id,
                            LinkedBuyerName = $"{linkage.Buyer.FirstName} {linkage.Buyer.LastName}",
                            Status = linkage.Status.ToString()
                        });
                    }
                }

                // Get buyer badges info
                viewModel.BuyerBadges = new List<BuyerBadgeViewModel>
                {
                    new BuyerBadgeViewModel
                    {
                        Id = buyer.Id,
                        BadgeName = buyer.Badge.ToString(),
                        Discount = buyer.Discount,
                        Progress = _buyerService.GetBadgeProgress(buyer),
                        ImageSource = $"/images/{buyer.Badge.ToString().ToLower()}.png"
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user data for user {UserId}", userId);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
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

        public async Task<IActionResult> EditAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var address = await _buyerAddressService.GetAddressByIdAsync(id);
                if (address == null)
                {
                    return NotFound();
                }

                var addressViewModel = new BuyerAddressViewModel
                {
                    Id = address.Id,
                    StreetLine = address.StreetLine,
                    City = address.City,
                    Country = address.Country,
                    PostalCode = address.PostalCode
                };

                return PartialView("_EditAddress", addressViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching address data for ID {AddressId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAddress(BuyerAddressViewModel addressViewModel)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditAddress", addressViewModel);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                // Get the current buyer
                var user = new User { UserId = userIdInt };
                var buyer = await _buyerService.GetBuyerByUser(user);

                // Convert view model to domain model
                var address = new Address
                {
                    Id = addressViewModel.Id,
                    StreetLine = addressViewModel.StreetLine,
                    City = addressViewModel.City,
                    Country = addressViewModel.Country,
                    PostalCode = addressViewModel.PostalCode
                };

                // Update or create the address
                if (address.Id > 0)
                {
                    await _buyerAddressService.UpdateAddressAsync(address);
                }
                else
                {
                    await _buyerAddressService.AddAddressAsync(address);
                }

                // Update buyer's billing address
                buyer.BillingAddress = address;

                // If the "Use Same Address" checkbox is checked, update the shipping address
                if (addressViewModel.UseSameAddress)
                {
                    buyer.ShippingAddress = address;
                    buyer.UseSameAddress = true;
                }
                else
                {
                    buyer.UseSameAddress = false;
                }

                // Save buyer info
                await _buyerService.SaveInfo(buyer);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving address data for user {UserId}", userId);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }


        public async Task<IActionResult> EditShippingAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var address = await _buyerAddressService.GetAddressByIdAsync(id);
                if (address == null)
                {
                    return NotFound();
                }

                var addressViewModel = new BuyerAddressViewModel
                {
                    Id = address.Id,
                    StreetLine = address.StreetLine,
                    City = address.City,
                    Country = address.Country,
                    PostalCode = address.PostalCode
                };

                return PartialView("_EditShippingAddress", addressViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching shipping address data for ID {AddressId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveShippingAddress(BuyerAddressViewModel shippingAddressViewModel)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditShippingAddress", shippingAddressViewModel);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                // Get the current buyer
                var user = new User { UserId = userIdInt };
                var buyer = await _buyerService.GetBuyerByUser(user);

                // Don't update shipping address if using same address as billing
                if (buyer.UseSameAddress)
                {
                    return RedirectToAction(nameof(Index));
                }

                // Convert view model to domain model
                var address = new Address
                {
                    Id = shippingAddressViewModel.Id,
                    StreetLine = shippingAddressViewModel.StreetLine,
                    City = shippingAddressViewModel.City,
                    Country = shippingAddressViewModel.Country,
                    PostalCode = shippingAddressViewModel.PostalCode
                };

                // Update or create the address
                if (address.Id > 0)
                {
                    await _buyerAddressService.UpdateAddressAsync(address);
                }
                else
                {
                    await _buyerAddressService.AddAddressAsync(address);
                }

                // Update buyer's shipping address
                buyer.ShippingAddress = address;
                await _buyerService.SaveInfo(buyer);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving shipping address data for user {UserId}", userId);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSameAddress(bool useSameAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                // Get the current buyer
                var user = new User { UserId = userIdInt };
                var buyer = await _buyerService.GetBuyerByUser(user);

                // Store previous shipping address if toggling to "use same"
                Address previousShippingAddress = null;
                if (useSameAddress && !buyer.UseSameAddress)
                {
                    previousShippingAddress = buyer.ShippingAddress;
                    buyer.ShippingAddress = buyer.BillingAddress;
                }
                else if (!useSameAddress && buyer.UseSameAddress)
                {
                    // Create new address for shipping if toggling to "different addresses"
                    buyer.ShippingAddress = previousShippingAddress ?? new Address
                    {
                        StreetLine = "",
                        City = "",
                        Country = "",
                        PostalCode = ""
                    };
                }

                buyer.UseSameAddress = useSameAddress;
                await _buyerService.SaveInfo(buyer);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling same address setting for user {UserId}", userId);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
