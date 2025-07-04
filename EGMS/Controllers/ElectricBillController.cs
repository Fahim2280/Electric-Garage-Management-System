using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using System.Text.Json;

namespace EGMS.Controllers
{
    public class ElectricBillController : Controller
    {
        private readonly IElectricBillService _electricBillService;

        public ElectricBillController(IElectricBillService electricBillService)
        {
            _electricBillService = electricBillService;
        }

        // GET: ElectricBill
        public async Task<IActionResult> Index()
        {
            var electricBills = await _electricBillService.GetAllElectricBillsAsync();
            return View(electricBills);
        }

        // GET: ElectricBill/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var electricBill = await _electricBillService.GetElectricBillByIdAsync(id);
            if (electricBill == null)
            {
                return NotFound();
            }
            return View(electricBill);
        }

        // GET: ElectricBill/Create
        public async Task<IActionResult> Create()
        {
            var customers = await _electricBillService.GetAllCustomersAsync();
            ViewBag.Customers = customers ?? new List<Customer>();

            // Return initialized model with today's date
            return View(new ElectricBillDTO
            {
                Date = DateTime.Today
            });
        }

        // POST: ElectricBill/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ElectricBillDTO electricBillDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _electricBillService.CreateElectricBillAsync(electricBillDto);
                if (result)
                {
                    TempData["SuccessMessage"] = "Electric bill created successfully with automatic calculations.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to create electric bill.");
            }

            ViewBag.Customers = await _electricBillService.GetAllCustomersAsync();
            return View(electricBillDto);
        }

        // GET: ElectricBill/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var electricBill = await _electricBillService.GetElectricBillByIdAsync(id);
            if (electricBill == null)
            {
                return NotFound();
            }

            ViewBag.Customers = await _electricBillService.GetAllCustomersAsync();
            return View(electricBill);
        }

        // POST: ElectricBill/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ElectricBillDTO electricBillDto)
        {
            if (id != electricBillDto.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _electricBillService.UpdateElectricBillAsync(electricBillDto);
                if (result)
                {
                    TempData["SuccessMessage"] = "Electric bill updated successfully with automatic calculations.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Failed to update electric bill.");
            }

            ViewBag.Customers = await _electricBillService.GetAllCustomersAsync();
            return View(electricBillDto);
        }

        // GET: ElectricBill/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var electricBill = await _electricBillService.GetElectricBillByIdAsync(id);
            if (electricBill == null)
            {
                return NotFound();
            }
            return View(electricBill);
        }

        // POST: ElectricBill/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _electricBillService.DeleteElectricBillAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Electric bill deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete electric bill.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ElectricBill/Customer/5
        public async Task<IActionResult> CustomerBills(int customerId)
        {
            var electricBills = await _electricBillService.GetElectricBillsByCustomerIdAsync(customerId);
            var customer = (await _electricBillService.GetAllCustomersAsync())
                .FirstOrDefault(c => c.C_ID == customerId);

            ViewBag.CustomerName = customer?.Name ?? "Unknown Customer";
            ViewBag.CustomerId = customerId;

            return View(electricBills);
        }

        // AJAX endpoint to get customer summary when customer is selected
        [HttpGet]
        public async Task<IActionResult> GetCustomerSummary(int customerId)
        {
            try
            {
                var summary = await _electricBillService.GetCustomerBillSummaryAsync(customerId);
                if (summary == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        customerId = summary.CustomerId,
                        customerName = summary.CustomerName,
                        lastMeterReading = summary.LastMeterReading,
                        previousDues = summary.PreviousDues,
                        lastBillDate = summary.LastBillDate?.ToString("yyyy-MM-dd")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving customer summary" });
            }
        }

        // AJAX endpoint to preview bill calculation
        [HttpPost]
        public async Task<IActionResult> PreviewBill([FromBody] PreviewBillRequest request)
        {
            try
            {
                var preview = await _electricBillService.PreviewElectricBillAsync(
                    request.CustomerId,
                    request.CurrentMeterReading,
                    request.RentBill);

                if (preview == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        customerId = preview.CustomerId,
                        customerName = preview.CustomerName,
                        previousMeterReading = preview.PreviousMeterReading,
                        currentMeterReading = preview.CurrentMeterReading,
                        consumedUnits = preview.ConsumedUnits,
                        electricBill = preview.ElectricBill,
                        rentBill = preview.RentBill,
                        previousDues = preview.PreviousDues,
                        totalBill = preview.TotalBill
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error calculating bill preview" });
            }
        }

        // Method to generate monthly bills for all customers
        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyBills(DateTime billDate)
        {
            try
            {
                var customers = await _electricBillService.GetAllCustomersAsync();
                int successCount = 0;
                int failCount = 0;

                foreach (var customer in customers)
                {
                    // Check if bill already exists for this customer for this month
                    var existingBills = await _electricBillService.GetElectricBillsByCustomerIdAsync(customer.C_ID);
                    bool billExists = existingBills.Any(b => b.Date.Year == billDate.Year && b.Date.Month == billDate.Month);

                    if (!billExists)
                    {
                        var summary = await _electricBillService.GetCustomerBillSummaryAsync(customer.C_ID);
                        if (summary != null)
                        {
                            // Create a bill with default values (assuming no change in meter reading and no payment)
                            var billDto = new ElectricBillDTO
                            {
                                Customer_ID = customer.C_ID,
                                Date = billDate,
                                Total_Unit = summary.LastMeterReading, // Same as last reading (no consumption)
                                Rent_Bill = 0, // Default rent bill
                                Clear_money = 0 // No payment
                            };

                            var result = await _electricBillService.CreateElectricBillAsync(billDto);
                            if (result)
                                successCount++;
                            else
                                failCount++;
                        }
                    }
                }

                TempData["SuccessMessage"] = $"Monthly bills generated successfully. {successCount} bills created, {failCount} failed.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error generating monthly bills.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // Request model for preview bill AJAX call
    public class PreviewBillRequest
    {
        public int CustomerId { get; set; }
        public decimal CurrentMeterReading { get; set; }
        public decimal RentBill { get; set; }
    }
}