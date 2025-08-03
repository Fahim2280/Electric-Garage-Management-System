using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using System.Text.Json;

namespace EGMS.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Create(int? id = null)
        {
            try
            {
                var customers = await _electricBillService.GetAllCustomersAsync();
                ViewBag.Customers = customers ?? new List<Customer>();

                var model = new ElectricBillDTO
                {
                    Date = DateTime.Today
                };

                // If customer ID is provided, pre-populate the form
                if (id.HasValue && id.Value > 0)
                {
                    var customer = customers?.FirstOrDefault(c => c.C_ID == id.Value);
                    if (customer != null)
                    {
                        model.Customer_ID = customer.C_ID;
                        model.CustomerName = customer.Name;
                        model.Previous_unit = customer.Previous_Unit;

                        // Get customer's bill summary to populate previous dues and last reading
                        var customerSummary = await _electricBillService.GetCustomerBillSummaryAsync(id.Value);
                        if (customerSummary != null)
                        {
                            model.Previous_unit = customerSummary.LastMeterReading;
                            model.Previous_duos = customerSummary.PreviousDues;
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading customer data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ElectricBill/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ElectricBillDTO electricBillDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Additional validation
                    if (electricBillDto.Customer_ID <= 0)
                    {
                        ModelState.AddModelError("Customer_ID", "Please select a customer.");
                    }

                    if (electricBillDto.Current_Unit < 0)
                    {
                        ModelState.AddModelError("Current_Unit", "Current unit reading cannot be negative.");
                    }

                    if (electricBillDto.Rent_Bill < 0)
                    {
                        ModelState.AddModelError("Rent_Bill", "Rent bill cannot be negative.");
                    }

                    if (electricBillDto.Loan < 0)
                    {
                        ModelState.AddModelError("Loan", "Loan amount cannot be negative.");
                    }

                    if (electricBillDto.Clear_money < 0)
                    {
                        ModelState.AddModelError("Clear_money", "Payment amount cannot be negative.");
                    }

                    // Validate that current reading is not less than previous reading
                    var customerSummary = await _electricBillService.GetCustomerBillSummaryAsync(electricBillDto.Customer_ID);
                    if (customerSummary != null && electricBillDto.Current_Unit < customerSummary.LastMeterReading)
                    {
                        ModelState.AddModelError("Current_Unit",
                            $"Current unit reading ({electricBillDto.Current_Unit}) cannot be less than the last meter reading ({customerSummary.LastMeterReading}).");
                    }

                    if (ModelState.IsValid)
                    {
                        var result = await _electricBillService.CreateElectricBillAsync(electricBillDto);
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Electric bill created successfully with automatic calculations.";

                            // Redirect back to customer details if customer ID is available
                            if (electricBillDto.Customer_ID > 0)
                            {
                                return RedirectToAction("Details", "Customer", new { id = electricBillDto.Customer_ID });
                            }

                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to create electric bill. Please check your input and try again.";
                        }
                    }
                }

                // If we got this far, something failed, redisplay form
                var customers = await _electricBillService.GetAllCustomersAsync();
                ViewBag.Customers = customers ?? new List<Customer>();

                // Re-populate customer name if available
                if (electricBillDto.Customer_ID > 0)
                {
                    var customer = customers?.FirstOrDefault(c => c.C_ID == electricBillDto.Customer_ID);
                    if (customer != null)
                    {
                        electricBillDto.CustomerName = customer.Name;
                    }
                }

                return View(electricBillDto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the electric bill. Please try again.";

                // Reload customers for the form
                try
                {
                    var customers = await _electricBillService.GetAllCustomersAsync();
                    ViewBag.Customers = customers ?? new List<Customer>();

                    // Re-populate customer name if available
                    if (electricBillDto.Customer_ID > 0)
                    {
                        var customer = customers?.FirstOrDefault(c => c.C_ID == electricBillDto.Customer_ID);
                        if (customer != null)
                        {
                            electricBillDto.CustomerName = customer.Name;
                        }
                    }
                }
                catch
                {
                    ViewBag.Customers = new List<Customer>();
                }

                return View(electricBillDto);
            }
        }

        // GET: ElectricBill/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var electricBill = await _electricBillService.GetElectricBillByIdAsync(id);
                if (electricBill == null)
                {
                    return NotFound();
                }

                ViewBag.Customers = await _electricBillService.GetAllCustomersAsync();
                return View(electricBill);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading bill data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
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

            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _electricBillService.UpdateElectricBillAsync(electricBillDto);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Electric bill updated successfully with automatic calculations.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update electric bill. Please try again.";
                    }
                }

                ViewBag.Customers = await _electricBillService.GetAllCustomersAsync();
                return View(electricBillDto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the electric bill. Please try again.";

                try
                {
                    ViewBag.Customers = await _electricBillService.GetAllCustomersAsync();
                }
                catch
                {
                    ViewBag.Customers = new List<Customer>();
                }

                return View(electricBillDto);
            }
        }

        // GET: ElectricBill/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var electricBill = await _electricBillService.GetElectricBillByIdAsync(id);
                if (electricBill == null)
                {
                    return NotFound();
                }
                return View(electricBill);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading bill data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ElectricBill/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the electric bill.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ElectricBill/Customer/5
        public async Task<IActionResult> CustomerBills(int customerId)
        {
            try
            {
                var electricBills = await _electricBillService.GetElectricBillsByCustomerIdAsync(customerId);
                var customer = (await _electricBillService.GetAllCustomersAsync())
                    .FirstOrDefault(c => c.C_ID == customerId);

                ViewBag.CustomerName = customer?.Name ?? "Unknown Customer";
                ViewBag.CustomerId = customerId;

                return View(electricBills);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading customer bills. Please try again.";
                return RedirectToAction(nameof(Index));
            }
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
                    request.RentBill,
                    request.Loan);

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
                        loan = preview.Loan,
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
                    try
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
                                    Current_Unit = summary.LastMeterReading, // Same as last reading (no consumption)
                                    Rent_Bill = 0, // Default rent bill
                                    Loan = 0, // Default loan amount
                                    Clear_money = 0 // No payment
                                };

                                var result = await _electricBillService.CreateElectricBillAsync(billDto);
                                if (result)
                                    successCount++;
                                else
                                    failCount++;
                            }
                            else
                            {
                                failCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"Monthly bills generated successfully. {successCount} bills created" +
                                                (failCount > 0 ? $", {failCount} failed" : "") + ".";
                }
                else
                {
                    TempData["ErrorMessage"] = "No bills were generated. " +
                                             (failCount > 0 ? $"{failCount} attempts failed." : "All customers may already have bills for this month.");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error generating monthly bills. Please try again.";
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
        public decimal Loan { get; set; }
    }
}