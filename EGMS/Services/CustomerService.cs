using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EGMS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user?.Id;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return new List<CustomerDTO>();

                var customers = await _context.Customers
                    .Where(c => c.UserId == currentUserId.Value)
                    .OrderByDescending(c => c.Created_Date)
                    .ToListAsync();

                Console.WriteLine($"Found {customers.Count} customers for user {currentUserId}");

                return customers.Select(c => new CustomerDTO
                {
                    C_ID = c.C_ID,
                    Name = c.Name,
                    F_name = c.F_name,
                    M_name = c.M_name,
                    Address = c.Address,
                    Mobile_number = c.Mobile_number,
                    NID_Number = c.NID_Number,
                    Previous_Unit = c.Previous_Unit.ToString(),
                    Advance_money = c.Advance_money.ToString(),
                    Created_Date = c.Created_Date
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllCustomersAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<CustomerDTO>();
            }
        }

        public async Task<IEnumerable<ElectricBillDTO>> GetCustomerBillsAsync(int customerId)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return new List<ElectricBillDTO>();

                // First check if the customer belongs to the current user
                var customerExists = await _context.Customers
                    .AnyAsync(c => c.C_ID == customerId && c.UserId == currentUserId.Value);

                if (!customerExists)
                    return new List<ElectricBillDTO>();

                var bills = await _context.ElectricBills
                    .Where(b => b.Customer_ID == customerId)
                    .Include(b => b.Customer)
                    .ToListAsync();

                return bills.Select(bill => new ElectricBillDTO
                {
                    ID = bill.ID,
                    Customer_ID = bill.Customer_ID,
                    Date = bill.Date,
                    Previous_unit = bill.Previous_unit,
                    Current_Unit = bill.Current_Unit,
                    Total_Unit = bill.Total_Unit,
                    Electric_bill = bill.Electric_bill,
                    Previous_duos = bill.Previous_duos,
                    Rent_Bill = bill.Rent_Bill,
                    Total_bill = bill.Total_bill,
                    Clear_money = bill.Clear_money,
                    Present_dues = bill.Present_dues
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCustomerBillsAsync: {ex.Message}");
                return new List<ElectricBillDTO>();
            }
        }

        public async Task<CustomerDTO?> GetCustomerByIdAsync(int id)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return null;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == id && c.UserId == currentUserId.Value);

                if (customer == null) return null;

                return new CustomerDTO
                {
                    C_ID = customer.C_ID,
                    Name = customer.Name,
                    F_name = customer.F_name,
                    M_name = customer.M_name,
                    Address = customer.Address,
                    Mobile_number = customer.Mobile_number,
                    NID_Number = customer.NID_Number,
                    Previous_Unit = customer.Previous_Unit.ToString(),
                    Advance_money = customer.Advance_money.ToString(),
                    Created_Date = customer.Created_Date
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCustomerByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateCustomerAsync(CustomerCreateDTO dto)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var customer = new Customer
                {
                    Name = dto.Name,
                    F_name = dto.F_name,
                    M_name = dto.M_name,
                    Address = dto.Address,
                    Mobile_number = dto.Mobile_number,
                    NID_Number = dto.NID_Number,
                    Previous_Unit = decimal.Parse(dto.Previous_Unit),
                    Advance_money = decimal.Parse(dto.Advance_money),
                    Created_Date = DateTime.UtcNow,
                    UserId = currentUserId.Value
                };

                _context.Customers.Add(customer);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Customer created by user {currentUserId}: {result > 0}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateCustomerAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(int id, CustomerDTO dto)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == id && c.UserId == currentUserId.Value);

                if (customer == null) return false;

                customer.Name = dto.Name;
                customer.F_name = dto.F_name;
                customer.M_name = dto.M_name;
                customer.Address = dto.Address;
                customer.Mobile_number = dto.Mobile_number;
                customer.NID_Number = dto.NID_Number;
                customer.Previous_Unit = decimal.Parse(dto.Previous_Unit);
                customer.Advance_money = decimal.Parse(dto.Advance_money);

                _context.Customers.Update(customer);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Customer updated by user {currentUserId}: {result > 0}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCustomerAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == id && c.UserId == currentUserId.Value);

                if (customer == null) return false;

                _context.Customers.Remove(customer);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Customer deleted by user {currentUserId}: {result > 0}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCustomerAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsNIDUniqueAsync(string nidNumber, int? excludeCustomerId = null)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var query = _context.Customers
                    .Where(c => c.NID_Number == nidNumber && c.UserId == currentUserId.Value);

                if (excludeCustomerId.HasValue)
                {
                    query = query.Where(c => c.C_ID != excludeCustomerId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsNIDUniqueAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsMobileUniqueAsync(string mobileNumber, int? excludeCustomerId = null)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var query = _context.Customers
                    .Where(c => c.Mobile_number == mobileNumber && c.UserId == currentUserId.Value);

                if (excludeCustomerId.HasValue)
                {
                    query = query.Where(c => c.C_ID != excludeCustomerId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsMobileUniqueAsync: {ex.Message}");
                return false;
            }
        }
    }
}