using Microsoft.AspNetCore.Mvc;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerFamilySyncController : Controller
    {
        // Mock data source for BuyerFamilySyncViewModel
        private static List<BuyerFamilySyncViewModel> _buyerFamilySyncs = new List<BuyerFamilySyncViewModel>
        {
            new BuyerFamilySyncViewModel { Id = 1, LinkedBuyerName = "John Doe", Status = "Confirmed" },
            new BuyerFamilySyncViewModel { Id = 2, LinkedBuyerName = "Jane Smith", Status = "Pending" },
            new BuyerFamilySyncViewModel { Id = 3, LinkedBuyerName = "Alice Johnson", Status = "Possible" }
        };

        // GET: BuyerFamilySync
        public IActionResult Index()
        {
            return View(_buyerFamilySyncs);
        }

        // GET: BuyerFamilySync/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerFamilySync = _buyerFamilySyncs.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySync == null)
            {
                return NotFound();
            }

            return View(buyerFamilySync);
        }

        // GET: BuyerFamilySync/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerFamilySync/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,LinkedBuyerName,Status")] BuyerFamilySyncViewModel buyerFamilySyncViewModel)
        {
            if (ModelState.IsValid)
            {
                buyerFamilySyncViewModel.Id = _buyerFamilySyncs.Max(b => b.Id) + 1; // Generate a new ID
                _buyerFamilySyncs.Add(buyerFamilySyncViewModel);
                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncViewModel);
        }

        // GET: BuyerFamilySync/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerFamilySync = _buyerFamilySyncs.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySync == null)
            {
                return NotFound();
            }

            return View(buyerFamilySync);
        }

        // POST: BuyerFamilySync/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,LinkedBuyerName,Status")] BuyerFamilySyncViewModel buyerFamilySyncViewModel)
        {
            if (id != buyerFamilySyncViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var buyerFamilySync = _buyerFamilySyncs.FirstOrDefault(b => b.Id == id);
                if (buyerFamilySync == null)
                {
                    return NotFound();
                }

                buyerFamilySync.LinkedBuyerName = buyerFamilySyncViewModel.LinkedBuyerName;
                buyerFamilySync.Status = buyerFamilySyncViewModel.Status;

                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncViewModel);
        }

        // GET: BuyerFamilySync/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buyerFamilySync = _buyerFamilySyncs.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySync == null)
            {
                return NotFound();
            }

            return View(buyerFamilySync);
        }

        // POST: BuyerFamilySync/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var buyerFamilySync = _buyerFamilySyncs.FirstOrDefault(b => b.Id == id);
            if (buyerFamilySync != null)
            {
                _buyerFamilySyncs.Remove(buyerFamilySync);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
