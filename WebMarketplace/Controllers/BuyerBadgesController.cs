using Microsoft.AspNetCore.Mvc;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerBadgesController : Controller
    {
        // Mock data source for BuyerBadgeViewModel
        private static List<BuyerBadgeViewModel> _buyerBadges = new List<BuyerBadgeViewModel>
        {
            new BuyerBadgeViewModel { Id = 1, BadgeName = "Bronze", Discount = 5, Progress = 20, ImageSource = "/images/bronze.png" },
            new BuyerBadgeViewModel { Id = 2, BadgeName = "Silver", Discount = 10, Progress = 50, ImageSource = "/images/silver.png" },
            new BuyerBadgeViewModel { Id = 3, BadgeName = "Gold", Discount = 15, Progress = 80, ImageSource = "/images/gold.png" }
        };

        // GET: BuyerBadges
        public IActionResult Index()
        {
            return View(_buyerBadges);
        }

        // GET: BuyerBadges/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerBadge = _buyerBadges.FirstOrDefault(b => b.Id == id);
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
        public IActionResult Create([Bind("Id,BadgeName,Discount,Progress,ImageSource")] BuyerBadgeViewModel buyerBadgeViewModel)
        {
            if (ModelState.IsValid)
            {
                buyerBadgeViewModel.Id = _buyerBadges.Max(b => b.Id) + 1; // Generate a new ID
                _buyerBadges.Add(buyerBadgeViewModel);
                return RedirectToAction(nameof(Index));
            }
            return View(buyerBadgeViewModel);
        }

        // GET: BuyerBadges/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerBadge = _buyerBadges.FirstOrDefault(b => b.Id == id);
            if (buyerBadge == null)
            {
                return NotFound();
            }

            return View(buyerBadge);
        }

        // POST: BuyerBadges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,BadgeName,Discount,Progress,ImageSource")] BuyerBadgeViewModel buyerBadgeViewModel)
        {
            if (id != buyerBadgeViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var buyerBadge = _buyerBadges.FirstOrDefault(b => b.Id == id);
                if (buyerBadge == null)
                {
                    return NotFound();
                }

                buyerBadge.BadgeName = buyerBadgeViewModel.BadgeName;
                buyerBadge.Discount = buyerBadgeViewModel.Discount;
                buyerBadge.Progress = buyerBadgeViewModel.Progress;
                buyerBadge.ImageSource = buyerBadgeViewModel.ImageSource;

                return RedirectToAction(nameof(Index));
            }
            return View(buyerBadgeViewModel);
        }

        // GET: BuyerBadges/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerBadge = _buyerBadges.FirstOrDefault(b => b.Id == id);
            if (buyerBadge == null)
            {
                return NotFound();
            }

            return View(buyerBadge);
        }

        // POST: BuyerBadges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var buyerBadge = _buyerBadges.FirstOrDefault(b => b.Id == id);
            if (buyerBadge != null)
            {
                _buyerBadges.Remove(buyerBadge);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
