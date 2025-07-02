using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
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
                    .Select(c => new CustomerDTO
                    {
                        C_ID = c.C_ID,
                        Name = c.Name,
                        F_name = c.F_name,
                        M_name = c.M_name,
                        Address = c.Address,
                        Mobile_number = c.Mobile_number,
                        NID_Number = c.NID_Number,
                        Created_Date = c.Created_Date
                    })
                    .ToListAsync();

                return customers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customers: {ex.Message}");
                return new List<CustomerDTO>();
            }
        }

        public async Task<CustomerDTO> GetCustomerByIdAsync(int id)
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
                    Created_Date = customer.Created_Date
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateCustomerAsync(CustomerCreateDTO dto)
        {
            try
            {
                // Check for duplicate NID
                if (await IsNIDUniqueAsync(dto.NID_Number) == false)
                    return false;

                // Check for duplicate Mobile
                if (await IsMobileUniqueAsync(dto.Mobile_number) == false)
                    return false;

                var customer = new Customer
                {
                    Name = dto.Name.Trim(),
                    F_name = dto.F_name.Trim(),
                    M_name = dto.M_name.Trim(),
                    Address = dto.Address.Trim(),
                    Mobile_number = dto.Mobile_number.Trim(),
                    NID_Number = dto.NID_Number.Trim(),
                    Created_Date = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating customer: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(int id, CustomerDTO dto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;

                // Check for duplicate NID (excluding current customer)
                if (await IsNIDUniqueAsync(dto.NID_Number, id) == false)
                    return false;

                // Check for duplicate Mobile (excluding current customer)
                if (await IsMobileUniqueAsync(dto.Mobile_number, id) == false)
                    return false;

                customer.Name = dto.Name.Trim();
                customer.F_name = dto.F_name.Trim();
                customer.M_name = dto.M_name.Trim();
                customer.Address = dto.Address.Trim();
                customer.Mobile_number = dto.Mobile_number.Trim();
                customer.NID_Number = dto.NID_Number.Trim();

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating customer: {ex.Message}");
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
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.C_ID == id);
        }

        public async Task<bool> IsNIDUniqueAsync(string nid, int? excludeId = null)
        {
            var query = _context.Customers.Where(c => c.NID_Number == nid);

            if (excludeId.HasValue)
                query = query.Where(c => c.C_ID != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<bool> IsMobileUniqueAsync(string mobile, int? excludeId = null)
        {
            var query = _context.Customers.Where(c => c.Mobile_number == mobile);

            if (excludeId.HasValue)
                query = query.Where(c => c.C_ID != excludeId.Value);

            return !await query.AnyAsync();
        }
    }
}