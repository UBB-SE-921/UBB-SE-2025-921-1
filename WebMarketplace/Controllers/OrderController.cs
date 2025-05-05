using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebMarketplace.Controllers
{
    public class OrderController : Controller
    {
        private readonly ITrackedOrderService _trackedOrderService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ITrackedOrderService trackedOrderService, IOrderService orderService, ILogger<OrderController> logger)
        {
            _trackedOrderService = trackedOrderService;
            _orderService = orderService;
            _logger = logger;
        }

        // GET: Order/Test
        public IActionResult Test()
        {
            return Content("Test action works!");
        }

        // GET: Order/OrderHistory
        public async Task<IActionResult> OrderHistory(int userId)
        {
            var orders = await _orderService.GetOrdersWithProductInfoAsync(userId);
            return View(orders);
        }

        // GET: Order/TrackOrder/{orderId}?admin={true/false}
        [HttpGet]
        [Route("Order/TrackOrder/{orderId:int}")]
        public async Task<IActionResult> TrackOrder(int orderId, bool admin = false)
        {
            _logger.LogInformation($"TrackOrder action called with orderId: {orderId}");
            
            var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(orderId);
            if (trackedOrder == null)
            {
                _logger.LogWarning($"Tracked order not found for orderId: {orderId}");
                return NotFound();
            }

            if (admin)
            {
                _logger.LogInformation($"Rendering TrackedOrderControl view for orderId: {orderId}");
                return View("TrackedOrderControl", trackedOrder);
            }
            else
            {
                _logger.LogInformation($"Rendering TrackedOrder view for orderId: {orderId}");
                return View("TrackedOrder", trackedOrder);
            }
        }

        // POST: Order/RevertCheckpoint
        [HttpPost]
        public async Task<IActionResult> RevertCheckpoint(int trackedOrderId)
        {
            try
            {
                var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderId);
                if (trackedOrder == null)
                {
                    return Json(new { success = false, message = "Tracked order not found" });
                }

                await _trackedOrderService.RevertToPreviousCheckpointAsync(trackedOrder);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error reverting checkpoint" });
            }
        }

        // POST: Order/UpdateDeliveryDate
        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryDate(int trackedOrderId, DateOnly newDate)
        {
            try
            {
                var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderId);
                if (trackedOrder == null)
                {
                    return Json(new { success = false, message = "Tracked order not found" });
                }

                await _trackedOrderService.UpdateTrackedOrderAsync(trackedOrderId, newDate, trackedOrder.CurrentStatus);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error updating delivery date" });
            }
        }

        // POST: Order/AddCheckpoint
        [HttpPost]
        public async Task<IActionResult> AddCheckpoint(int trackedOrderId, OrderCheckpoint checkpoint)
        {
            try
            {
                checkpoint.TrackedOrderID = trackedOrderId;
                await _trackedOrderService.AddOrderCheckpointAsync(checkpoint);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error adding checkpoint" });
            }
        }

        // POST: Order/UpdateCheckpoint
        [HttpPost]
        public async Task<IActionResult> UpdateCheckpoint(int trackedOrderId, OrderCheckpoint checkpoint)
        {
            try
            {
                await _trackedOrderService.UpdateOrderCheckpointAsync(
                    checkpoint.CheckpointID,
                    checkpoint.Timestamp,
                    checkpoint.Location,
                    checkpoint.Description,
                    checkpoint.Status);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error updating checkpoint" });
            }
        }
    }
} 