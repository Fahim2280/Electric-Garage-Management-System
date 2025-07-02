using EGMS.DTOs;
using EGMS.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EGMS.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View("Index", customers ?? new List<CustomerDTO>());
        }


        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View(new CustomerCreateDTO());
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Check for duplicate NID
            if (!await _customerService.IsNIDUniqueAsync(dto.NID_Number))
            {
                ModelState.AddModelError("NID_Number", "This NID number already exists.");
                return View(dto);
            }

            // Check for duplicate Mobile
            if (!await _customerService.IsMobileUniqueAsync(dto.Mobile_number))
            {
                ModelState.AddModelError("Mobile_number", "This mobile number already exists.");
                return View(dto);
            }

            var result = await _customerService.CreateCustomerAsync(dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Create)); // Redirect to same page
            }

            ModelState.AddModelError("", "Failed to create customer. Please try again.");
            return View(dto);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data submitted.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _customerService.IsNIDUniqueAsync(dto.NID_Number, dto.C_ID))
            {
                TempData["ErrorMessage"] = "NID number already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _customerService.IsMobileUniqueAsync(dto.Mobile_number, dto.C_ID))
            {
                TempData["ErrorMessage"] = "Mobile number already exists.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _customerService.UpdateCustomerAsync(dto.C_ID, dto);
            TempData["SuccessMessage"] = result ? "Customer updated successfully." : "Update failed.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete customer.";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}