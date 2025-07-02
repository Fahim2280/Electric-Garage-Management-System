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

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
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
        public async Task<IActionResult> Edit(int id, CustomerDTO dto)
        {
            if (id != dto.C_ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Check for duplicate NID (excluding current customer)
            if (!await _customerService.IsNIDUniqueAsync(dto.NID_Number, id))
            {
                ModelState.AddModelError("NID_Number", "This NID number already exists.");
                return View(dto);
            }

            // Check for duplicate Mobile (excluding current customer)
            if (!await _customerService.IsMobileUniqueAsync(dto.Mobile_number, id))
            {
                ModelState.AddModelError("Mobile_number", "This mobile number already exists.");
                return View(dto);
            }

            var result = await _customerService.UpdateCustomerAsync(id, dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update customer. Please try again.");
            return View(dto);
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
        [HttpPost, ActionName("Delete")]
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