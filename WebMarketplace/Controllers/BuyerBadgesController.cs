using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMarketplace.Models;
using Server.DBConnection;

namespace WebMarketplace.Controllers
{
    public class BuyerBadgesController : Controller
    {
        private readonly MarketPlaceDbContext _context;

        public BuyerBadgesController(MarketPlaceDbContext context)
        {
            _context = context;
        }

        // GET: BuyerBadges
        public async Task<IActionResult> Index()
        {
            return View(await _context.BuyerBadges.ToListAsync());
        }

        // GET: BuyerBadges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerBadge = await _context.BuyerBadges
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buyerBadge == null)
            {
                return NotFound();
            }

            return View(buyerBadge);
        }

        // GET: BuyerBadges/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerBadges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BadgeName,Discount,Progress,ImageSource")] BuyerBadges buyerBadge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(buyerBadge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(buyerBadge);
        }

        // GET: BuyerBadges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerBadge = await _context.BuyerBadges.FindAsync(id);
            if (buyerBadge == null)
            {
                return NotFound();
            }
            return View(buyerBadge);
        }

        // POST: BuyerBadges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BadgeName,Discount,Progress,ImageSource")] BuyerBadge buyerBadge)
        {
            if (id != buyerBadge.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buyerBadge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuyerBadgeExists(buyerBadge.Id))
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
            return View(buyerBadge);
        }

        // GET: BuyerBadges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerBadge = await _context.BuyerBadges
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buyerBadge == null)
            {
                return NotFound();
            }

            return View(buyerBadge);
        }

        // POST: BuyerBadges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var buyerBadge = await _context.BuyerBadges.FindAsync(id);
            if (buyerBadge != null)
            {
                _context.BuyerBadges.Remove(buyerBadge);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BuyerBadgeExists(int id)
        {
            return _context.BuyerBadgeViewModel.Any(e => e.Id == id);
        }
    }
}
