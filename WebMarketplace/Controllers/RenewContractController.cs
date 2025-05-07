using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebMarketplace.Controllers
{
    public class RenewContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IPDFService _pdfService;
        private readonly IContractRenewalService _renewalService;
        private readonly INotificationContentService _notificationService;

        // Default buyer and seller IDs
        private const int DefaultBuyerId = 2;
        private const int DefaultSellerId = 5; // Placeholder for actual seller ID

        public RenewContractController(
            IContractService contractService,
            IPDFService pdfService,
            IContractRenewalService renewalService,
            INotificationContentService notificationService)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _renewalService = renewalService ?? throw new ArgumentNullException(nameof(renewalService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<IActionResult> Index()
        {
            int buyerId = GetBuyerId();
            var allContracts = await _contractService.GetContractsByBuyerAsync(buyerId);
            var activeContracts = new List<IContract>();

            // Filter for active and renewed contracts
            foreach (var contract in allContracts)
            {
                if (contract.ContractStatus == "ACTIVE" || contract.ContractStatus == "RENEWED")
                {
                    activeContracts.Add(contract);
                }
            }

            ViewBag.BuyerId = buyerId;
            ViewBag.SellerId = DefaultSellerId;
            return View(activeContracts);
        }

        [HttpGet]
        public async Task<IActionResult> GetContractDetails(long contractId)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(contractId);
                var details = await _contractService.GetProductDetailsByContractIdAsync(contractId);

                bool isRenewalPeriodValid = false;

                if (details.HasValue && details.Value.EndDate.HasValue)
                {
                    DateTime oldEndDate = details.Value.EndDate.Value;
                    DateTime currentDate = DateTime.Now.Date;
                    int daysUntilEnd = (oldEndDate - currentDate).Days;
                    isRenewalPeriodValid = daysUntilEnd <= 7 && daysUntilEnd >= 2;
                }

                bool isAlreadyRenewed = await _renewalService.HasContractBeenRenewedAsync(contractId);

                return Json(new
                {
                    success = true,
                    startDate = details?.StartDate?.ToString("MM/dd/yyyy"),
                    endDate = details?.EndDate?.ToString("MM/dd/yyyy"),
                    price = details?.price,
                    name = details?.name,
                    renewalAllowed = !isAlreadyRenewed && isRenewalPeriodValid,
                    status = isRenewalPeriodValid ? "Available for renewal" : "Not available for renewal",
                    contractContent = contract?.ContractContent
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenewContract(long contractId, DateTime newEndDate, int buyerId, int productId, int sellerId)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(contractId);
                if (contract == null)
                {
                    return Json(new { success = false, message = "Contract not found" });
                }

                // Check if already renewed
                bool isRenewed = await _renewalService.HasContractBeenRenewedAsync(contractId);
                if (isRenewed)
                {
                    return Json(new { success = false, message = "This contract has already been renewed." });
                }

                // Check validity period
                var details = await _contractService.GetProductDetailsByContractIdAsync(contractId);
                if (!details.HasValue || !details.Value.EndDate.HasValue)
                {
                    return Json(new { success = false, message = "Could not retrieve contract dates." });
                }

                DateTime oldEndDate = details.Value.EndDate.Value;
                DateTime currentDate = DateTime.Now.Date;
                int daysUntilEnd = (oldEndDate - currentDate).Days;
                bool isRenewalPeriodValid = daysUntilEnd <= 7 && daysUntilEnd >= 2;

                if (!isRenewalPeriodValid)
                {
                    return Json(new { success = false, message = "Contract is not in a valid renewal period (between 2 and 7 days before end date)." });
                }

                // Check if new end date is after old end date
                if (newEndDate <= oldEndDate)
                {
                    return Json(new { success = false, message = "New end date must be after the current end date." });
                }

                // Check renewal limit
                bool canSellerApprove = contract.RenewalCount < 1;
                if (!canSellerApprove)
                {
                    return Json(new { success = false, message = "Renewal not allowed: seller limit exceeded." });
                }

                // Generate contract content and PDF
                string contractContent = $"Renewed Contract for Order {contract.OrderID}.\nOriginal Contract ID: {contract.ContractID}.\nNew End Date: {newEndDate:dd/MM/yyyy}";

                // For simplicity, we'll create a placeholder for PDF creation in web context
                byte[] pdfContent = System.Text.Encoding.UTF8.GetBytes(contractContent);
                int newPdfId = await _pdfService.InsertPdfAsync(pdfContent);

                // Create renewed contract
                var updatedContract = new Contract
                {
                    OrderID = contract.OrderID,
                    ContractStatus = "RENEWED",
                    ContractContent = contractContent,
                    RenewalCount = contract.RenewalCount + 1,
                    PredefinedContractID = contract.PredefinedContractID,
                    PDFID = newPdfId,
                    RenewedFromContractID = contract.ContractID,
                    AdditionalTerms = contract.AdditionalTerms ?? "Standard renewal terms apply" // FIX: Copy from original or set default
                };

                await _renewalService.AddRenewedContractAsync(updatedContract);

                // Send notifications
                var now = DateTime.Now;
                _notificationService.AddNotification(new ContractRenewalRequestNotification(sellerId, now, (int)contract.ContractID));
                _notificationService.AddNotification(new ContractRenewalAnswerNotification(buyerId, now, (int)contract.ContractID, true));
                _notificationService.AddNotification(new ContractRenewalWaitlistNotification(999, now, productId));

                return Json(new { success = true, message = "Contract renewed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // HARDCODED, NEED TO BE REPLACED WITH REAL USER ID
        private int GetBuyerId()
        {
            // In a real application, you would get this from the current user's claims
            // Here we're using a default value for testing
            return DefaultBuyerId;
        }
    }
}
