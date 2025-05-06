using Microsoft.AspNetCore.Mvc;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerFamilySyncItemController : Controller
    {
        // Mock data source for BuyerFamilySyncItemViewModel
        private static List<BuyerFamilySyncItemViewModel> _buyerFamilySyncItems = new List<BuyerFamilySyncItemViewModel>
        {
            new BuyerFamilySyncItemViewModel { Id = 1, DisplayName = "John Doe", Status = "Confirmed" },
            new BuyerFamilySyncItemViewModel { Id = 2, DisplayName = "Jane Smith", Status = "Pending" },
            new BuyerFamilySyncItemViewModel { Id = 3, DisplayName = "Alice Johnson", Status = "Possible" }
        };

        // GET: BuyerFamilySyncItem
        public IActionResult Index()
        {
            return View(_buyerFamilySyncItems);
        }

        // GET: BuyerFamilySyncItem/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerFamilySyncItem = _buyerFamilySyncItems.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySyncItem == null)
            {
                return NotFound();
            }

            return View(buyerFamilySyncItem);
        }

        // GET: BuyerFamilySyncItem/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerFamilySyncItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,DisplayName,Status")] BuyerFamilySyncItemViewModel buyerFamilySyncItemViewModel)
        {
            if (ModelState.IsValid)
            {
                buyerFamilySyncItemViewModel.Id = _buyerFamilySyncItems.Max(b => b.Id) + 1; // Generate a new ID
                _buyerFamilySyncItems.Add(buyerFamilySyncItemViewModel);
                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncItemViewModel);
        }

        // GET: BuyerFamilySyncItem/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerFamilySyncItem = _buyerFamilySyncItems.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySyncItem == null)
            {
                return NotFound();
            }

            return View(buyerFamilySyncItem);
        }

        // POST: BuyerFamilySyncItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,DisplayName,Status")] BuyerFamilySyncItemViewModel buyerFamilySyncItemViewModel)
        {
            if (id != buyerFamilySyncItemViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var buyerFamilySyncItem = _buyerFamilySyncItems.FirstOrDefault(b => b.Id == id);
                if (buyerFamilySyncItem == null)
                {
                    return NotFound();
                }

                buyerFamilySyncItem.DisplayName = buyerFamilySyncItemViewModel.DisplayName;
                buyerFamilySyncItem.Status = buyerFamilySyncItemViewModel.Status;

                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncItemViewModel);
        }

        // GET: BuyerFamilySyncItem/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerFamilySyncItem = _buyerFamilySyncItems.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySyncItem == null)
            {
                return NotFound();
            }

            return View(buyerFamilySyncItem);
        }

        // POST: BuyerFamilySyncItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var buyerFamilySyncItem = _buyerFamilySyncItems.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySyncItem != null)
            {
                _buyerFamilySyncItems.Remove(buyerFamilySyncItem);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
