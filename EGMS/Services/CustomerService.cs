using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EGMS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .OrderByDescending(c => c.Created_Date)
                    .ToListAsync();

                Console.WriteLine($"Found {customers.Count} customers in database");

                return customers.Select(c => new CustomerDTO
                {
                    C_ID = c.C_ID,
                    Name = c.Name,
                    F_name = c.F_name,
                    M_name = c.M_name,
                    Address = c.Address,
                    Mobile_number = c.Mobile_number,
                    NID_Number = c.NID_Number,
                    Previous_Unit = c.Previous_Unit.ToString(), // Fix: Convert decimal to string
                    Advance_money = c.Advance_money.ToString(), // Fix: Convert decimal to string
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
                    // Add other properties as needed
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
                var customer = await _context.Customers.FindAsync(id);
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
                    Previous_Unit = customer.Previous_Unit.ToString(), // Fix: Convert decimal to string
                    Advance_money = customer.Advance_money.ToString(), // Fix: Convert decimal to string
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
                var customer = new Customer
                {
                    Name = dto.Name,
                    F_name = dto.F_name,
                    M_name = dto.M_name,
                    Address = dto.Address,
                    Mobile_number = dto.Mobile_number,
                    NID_Number = dto.NID_Number,
                    Previous_Unit = decimal.Parse(dto.Previous_Unit), // Fix: Convert string to decimal
                    Advance_money = decimal.Parse(dto.Advance_money), // Fix: Convert string to decimal
                    Created_Date = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Customer created: {result > 0}");
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
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;

                customer.Name = dto.Name;
                customer.F_name = dto.F_name;
                customer.M_name = dto.M_name;
                customer.Address = dto.Address;
                customer.Mobile_number = dto.Mobile_number;
                customer.NID_Number = dto.NID_Number;

                // Fix for CS0029: Convert string to decimal using decimal.Parse
                customer.Previous_Unit = decimal.Parse(dto.Previous_Unit);
                customer.Advance_money = decimal.Parse(dto.Advance_money);

                _context.Customers.Update(customer);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Customer updated: {result > 0}");
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
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;

                _context.Customers.Remove(customer);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Customer deleted: {result > 0}");
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
                var query = _context.Customers.Where(c => c.NID_Number == nidNumber);

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
                var query = _context.Customers.Where(c => c.Mobile_number == mobileNumber);

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