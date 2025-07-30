using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EGMS.Models;
using EGMS.Interface;
using EGMS.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace EGMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICustomerService _customerService;
        private readonly IElectricBillService _electricBillService;

        public HomeController(ILogger<HomeController> logger,
                            ICustomerService customerService,
                            IElectricBillService electricBillService)
        {
            _logger = logger;
            _customerService = customerService;
            _electricBillService = electricBillService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get current user's name for display
                var currentUsername = User?.Identity?.Name ?? "Unknown User";

                // Get all customers for the current user
                // Note: Make sure ICustomerService.GetAllCustomersAsync() filters by current user
                var customers = await _customerService.GetAllCustomersAsync();

                // If no customers found, show empty dashboard
                if (customers == null || !customers.Any())
                {
                    ViewBag.TotalAdvanceMoney = 0;
                    ViewBag.TotalPresentDues = 0;
                    ViewBag.Balance = 0;
                    ViewBag.CurrentUser = currentUsername;
                    ViewBag.CustomerCount = 0;
                    ViewBag.Message = "No customers found for the current user.";

                    _logger.LogInformation($"No customers found for user: {currentUsername}");
                    return View(new List<CustomerDashboardDTO>());
                }

                // Create a list to hold customer dashboard data
                var customerDashboardList = new List<CustomerDashboardDTO>();

                // Variables for balance calculation
                decimal totalAdvanceMoney = 0;
                decimal totalPresentDues = 0;
                int customersWithBills = 0;

                foreach (var customer in customers)
                {
                    // Get the latest bill for each customer (this already filters by user in ElectricBillService)
                    var latestBill = await _electricBillService.GetLatestElectricBillByCustomerIdAsync(customer.C_ID);

                    // Calculate advance money for this customer
                    decimal customerAdvanceMoney = decimal.Parse(customer.Advance_money);
                    totalAdvanceMoney += customerAdvanceMoney;

                    // Calculate present dues for this customer
                    decimal customerPresentDues = latestBill?.Present_dues ?? customerAdvanceMoney;
                    totalPresentDues += customerPresentDues;

                    // Count customers with bills
                    if (latestBill != null)
                    {
                        customersWithBills++;
                    }

                    var dashboardItem = new CustomerDashboardDTO
                    {
                        C_ID = customer.C_ID,
                        Name = customer.Name,
                        Mobile_number = customer.Mobile_number,
                        Present_dues = customerPresentDues,
                        LastBillDate = latestBill?.Date ?? customer.Created_Date,
                        HasBills = latestBill != null
                    };

                    customerDashboardList.Add(dashboardItem);
                }

                // Calculate the balance
                decimal balance = totalAdvanceMoney - totalPresentDues;

                // Order by last bill date (most recent first)
                customerDashboardList = customerDashboardList
                    .OrderByDescending(x => x.LastBillDate)
                    .ToList();

                // Pass user-specific data to the view
                ViewBag.TotalAdvanceMoney = totalAdvanceMoney;
                ViewBag.TotalPresentDues = totalPresentDues;
                ViewBag.Balance = balance;
                ViewBag.CurrentUser = currentUsername;
                ViewBag.CustomerCount = customers.Count();
                ViewBag.CustomersWithBills = customersWithBills;
                ViewBag.CustomersWithoutBills = customers.Count() - customersWithBills;

                // Log the balance calculation for debugging with user info
                _logger.LogInformation($"User: {currentUsername} - Balance Calculation - " +
                                     $"Total Customers: {customers.Count()}, " +
                                     $"Total Advance: {totalAdvanceMoney:C}, " +
                                     $"Total Present Dues: {totalPresentDues:C}, " +
                                     $"Balance: {balance:C}");

                return View(customerDashboardList);
            }
            catch (Exception ex)
            {
                var currentUsername = User?.Identity?.Name ?? "Unknown User";
                _logger.LogError(ex, $"Error occurred while loading customer dashboard for user: {currentUsername}");

                // Set error state ViewBag values
                ViewBag.TotalAdvanceMoney = 0;
                ViewBag.TotalPresentDues = 0;
                ViewBag.Balance = 0;
                ViewBag.CurrentUser = currentUsername;
                ViewBag.CustomerCount = 0;
                ViewBag.ErrorMessage = "An error occurred while loading the dashboard.";

                return View(new List<CustomerDashboardDTO>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}