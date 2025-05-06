using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.DBConnection;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerAddressesController : Controller
    {
        private readonly MarketPlaceDbContext _context;

        public BuyerAddressesController(MarketPlaceDbContext context)
        {
            _context = context;
        }

        // GET: BuyerAddresses
        public async Task<IActionResult> Index()
        {
            var addresses = await _context.Addresses.ToListAsync(); // Use the correct DbSet
            return View(addresses);
        }

        // GET: BuyerAddresses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerAddresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BuyerAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var address = new Address
                {
                    StreetLine = model.StreetLine,
                    PostalCode = model.PostalCode,
                    City = model.City,
                    Country = model.Country
                };

                _context.Addresses.Add(address); // Use the Addresses DbSet
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: BuyerAddresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses.FindAsync(id); // Use the Addresses DbSet
            if (address == null)
            {
                return NotFound();
            }

            var model = new BuyerAddressViewModel
            {
                Id = address.Id,
                StreetLine = address.StreetLine,
                PostalCode = address.PostalCode,
                City = address.City,
                Country = address.Country
            };

            return View(model);
        }

        // POST: BuyerAddresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BuyerAddressViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var address = await _context.Addresses.FindAsync(id); // Use the Addresses DbSet
                    if (address == null)
                    {
                        return NotFound();
                    }

                    address.StreetLine = model.StreetLine;
                    address.PostalCode = model.PostalCode;
                    address.City = model.City;
                    address.Country = model.Country;

                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        private bool AddressExists(int id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}
