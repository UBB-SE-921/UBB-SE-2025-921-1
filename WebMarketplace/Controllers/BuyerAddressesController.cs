using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMarketplace.Models;
using Server.DBConnection;

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
            return View(await _context.Addresses.ToListAsync());
        }

        // GET: BuyerAddresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerAddress = await _context.Addresses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buyerAddress == null)
            {
                return NotFound();
            }

            return View(buyerAddress);
        }

        // GET: BuyerAddresses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerAddresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StreetLine,City,Country,PostalCode")] BuyerAddressViewModel buyerAddress)
        {
            if (ModelState.IsValid)
            {
                _context.Add(buyerAddress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(buyerAddress);
        }

        // GET: BuyerAddresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerAddress = await _context.Addresses.FindAsync(id);
            if (buyerAddress == null)
            {
                return NotFound();
            }
            return View(buyerAddress);
        }

        // POST: BuyerAddresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StreetLine,City,Country,PostalCode")] BuyerAddressViewModel buyerAddress)
        {
            if (id != buyerAddress.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buyerAddress);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuyerAddressExists(buyerAddress.Id))
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
            return View(buyerAddress);
        }

        // GET: BuyerAddresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerAddress = await _context.Addresses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buyerAddress == null)
            {
                return NotFound();
            }

            return View(buyerAddress);
        }

        // POST: BuyerAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var buyerAddress = await _context.Addresses.FindAsync(id);
            if (buyerAddress != null)
            {
                _context.Addresses.Remove(buyerAddress);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BuyerAddressExists(int id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}
