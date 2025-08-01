﻿using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EGMS.Services
{
    public class ElectricBillService : IElectricBillService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const decimal RATE_PER_UNIT = 15; // 15 taka per unit

        public ElectricBillService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<IEnumerable<ElectricBillDTO>> GetAllElectricBillsAsync()
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return new List<ElectricBillDTO>();

                var electricBills = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .Where(e => e.Customer.UserId == currentUserId.Value)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();

                return electricBills.Select(e => new ElectricBillDTO
                {
                    ID = e.ID,
                    Customer_ID = e.Customer_ID,
                    Date = e.Date,
                    Previous_unit = e.Previous_unit,
                    Current_Unit = e.Current_Unit,
                    Total_Unit = e.Total_Unit,
                    Electric_bill = e.Electric_bill,
                    Previous_duos = e.Previous_duos,
                    Rent_Bill = e.Rent_Bill,
                    Total_bill = e.Total_bill,
                    Clear_money = e.Clear_money,
                    Present_dues = e.Present_dues,
                    CustomerName = e.Customer.Name
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllElectricBillsAsync: {ex.Message}");
                return new List<ElectricBillDTO>();
            }
        }

        public async Task<ElectricBillDTO> GetElectricBillByIdAsync(int id)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return null;

                var electricBill = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.ID == id && e.Customer.UserId == currentUserId.Value);

                if (electricBill == null) return null;

                return new ElectricBillDTO
                {
                    ID = electricBill.ID,
                    Customer_ID = electricBill.Customer_ID,
                    Date = electricBill.Date,
                    Previous_unit = electricBill.Previous_unit,
                    Current_Unit = electricBill.Current_Unit,
                    Total_Unit = electricBill.Total_Unit,
                    Electric_bill = electricBill.Electric_bill,
                    Previous_duos = electricBill.Previous_duos,
                    Rent_Bill = electricBill.Rent_Bill,
                    Total_bill = electricBill.Total_bill,
                    Clear_money = electricBill.Clear_money,
                    Present_dues = electricBill.Present_dues,
                    CustomerName = electricBill.Customer.Name
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetElectricBillByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<ElectricBillDTO>> GetElectricBillsByCustomerIdAsync(int customerId)
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

                var electricBills = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .Where(e => e.Customer_ID == customerId && e.Customer.UserId == currentUserId.Value)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();

                return electricBills.Select(e => new ElectricBillDTO
                {
                    ID = e.ID,
                    Customer_ID = e.Customer_ID,
                    Date = e.Date,
                    Previous_unit = e.Previous_unit,
                    Current_Unit = e.Current_Unit,
                    Total_Unit = e.Total_Unit,
                    Electric_bill = e.Electric_bill,
                    Previous_duos = e.Previous_duos,
                    Rent_Bill = e.Rent_Bill,
                    Total_bill = e.Total_bill,
                    Clear_money = e.Clear_money,
                    Present_dues = e.Present_dues,
                    CustomerName = e.Customer.Name
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetElectricBillsByCustomerIdAsync: {ex.Message}");
                return new List<ElectricBillDTO>();
            }
        }

        public async Task<bool> CreateElectricBillAsync(ElectricBillDTO electricBillDto)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                // Get customer information and verify it belongs to current user
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == electricBillDto.Customer_ID && c.UserId == currentUserId.Value);

                if (customer == null)
                {
                    Console.WriteLine($"Customer with ID {electricBillDto.Customer_ID} not found or doesn't belong to current user");
                    return false;
                }

                // Get the last bill for this customer (chronologically)
                var lastBill = await _context.ElectricBills
                    .Where(e => e.Customer_ID == electricBillDto.Customer_ID && e.Customer.UserId == currentUserId.Value)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefaultAsync();

                decimal previousUnit;
                decimal previousDues;

                if (lastBill == null)
                {
                    // FIRST BILL: Get values from Customer table
                    Console.WriteLine("This is the first bill for this customer");
                    previousUnit = customer.Previous_Unit;
                    previousDues = customer.Advance_money; // Advance money becomes previous dues
                }
                else
                {
                    // SUBSEQUENT BILLS: Get values from last bill
                    Console.WriteLine("Getting values from last bill");
                    previousUnit = lastBill.Current_Unit;     // Current unit from last bill becomes previous unit
                    previousDues = lastBill.Present_dues;     // Present dues from last bill becomes previous dues
                }

                Console.WriteLine($"Previous Unit: {previousUnit}, Previous Dues: {previousDues}");

                // CORRECTED BUSINESS LOGIC CALCULATIONS:
                // 1. Current_Unit - Previous_unit = Total_Unit (consumed units)
                decimal totalUnit = electricBillDto.Current_Unit - previousUnit;

                // 2. Total_Unit * 15 = Electric_bill
                decimal electricBillAmount = totalUnit * RATE_PER_UNIT;

                // 3. Previous_duos + Electric_bill + Rent_Bill = Total_bill
                decimal totalBill = previousDues + electricBillAmount + electricBillDto.Rent_Bill;

                // 4. Total_bill - Clear_money = Present_dues
                decimal presentDues = totalBill - electricBillDto.Clear_money;

                Console.WriteLine($"Total Unit (Consumed): {totalUnit}");
                Console.WriteLine($"Electric Bill Amount: {electricBillAmount}");
                Console.WriteLine($"Total Bill: {totalBill}");
                Console.WriteLine($"Present Dues: {presentDues}");

                // Validation: Current unit should not be less than previous unit
                if (totalUnit < 0)
                {
                    Console.WriteLine("Error: Current unit cannot be less than previous unit");
                    return false;
                }

                var electricBillEntity = new ElectricBill
                {
                    Customer_ID = electricBillDto.Customer_ID,
                    Date = DateTime.Now,
                    Previous_unit = previousUnit,               // From customer or last bill
                    Current_Unit = electricBillDto.Current_Unit, // Current meter reading (input)
                    Total_Unit = totalUnit,                     // Calculated: consumed units
                    Electric_bill = electricBillAmount,         // Calculated: consumed units × 15
                    Previous_duos = previousDues,               // From customer advance or last bill dues
                    Rent_Bill = electricBillDto.Rent_Bill,      // Input from user
                    Total_bill = totalBill,                     // Calculated: previous dues + electric bill + rent bill
                    Clear_money = electricBillDto.Clear_money,  // Input from user (amount paid)
                    Present_dues = presentDues                  // Calculated: total bill - clear money
                };

                _context.ElectricBills.Add(electricBillEntity);
                await _context.SaveChangesAsync();

                Console.WriteLine("Electric bill created successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating electric bill: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateElectricBillAsync(ElectricBillDTO electricBillDto)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var electricBillEntity = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.ID == electricBillDto.ID && e.Customer.UserId == currentUserId.Value);

                if (electricBillEntity == null) return false;

                // Get customer information and verify it belongs to current user
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == electricBillDto.Customer_ID && c.UserId == currentUserId.Value);

                if (customer == null) return false;

                // Find the previous bill (chronologically before this bill's date)
                var previousBill = await _context.ElectricBills
                    .Where(e => e.Customer_ID == electricBillDto.Customer_ID &&
                               e.Date < electricBillDto.Date &&
                               e.ID != electricBillDto.ID &&
                               e.Customer.UserId == currentUserId.Value)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefaultAsync();

                decimal previousUnit;
                decimal previousDues;

                if (previousBill == null)
                {
                    // This is the first bill or no previous bill exists
                    previousUnit = customer.Previous_Unit;
                    previousDues = customer.Advance_money;
                }
                else
                {
                    // Get from previous bill
                    previousUnit = previousBill.Current_Unit;
                    previousDues = previousBill.Present_dues;
                }

                // CORRECTED BUSINESS LOGIC CALCULATIONS:
                decimal totalUnit = electricBillDto.Current_Unit - previousUnit;
                decimal electricBillAmount = totalUnit * RATE_PER_UNIT;
                decimal totalBill = previousDues + electricBillAmount + electricBillDto.Rent_Bill;
                decimal presentDues = totalBill - electricBillDto.Clear_money;

                // Validation: Current unit should not be less than previous unit
                if (totalUnit < 0)
                {
                    Console.WriteLine("Error: Current unit cannot be less than previous unit");
                    return false;
                }

                // Update the entity
                electricBillEntity.Customer_ID = electricBillDto.Customer_ID;
                electricBillEntity.Date = electricBillDto.Date;
                electricBillEntity.Previous_unit = previousUnit;
                electricBillEntity.Current_Unit = electricBillDto.Current_Unit;
                electricBillEntity.Total_Unit = totalUnit;
                electricBillEntity.Electric_bill = electricBillAmount;
                electricBillEntity.Previous_duos = previousDues;
                electricBillEntity.Rent_Bill = electricBillDto.Rent_Bill;
                electricBillEntity.Total_bill = totalBill;
                electricBillEntity.Clear_money = electricBillDto.Clear_money;
                electricBillEntity.Present_dues = presentDues;

                await _context.SaveChangesAsync();

                // Update subsequent bills if any
                await UpdateSubsequentBillsAsync(electricBillDto.Customer_ID, electricBillDto.Date);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating electric bill: {ex.Message}");
                return false;
            }
        }

        // Update all bills that come after a specific date
        private async Task UpdateSubsequentBillsAsync(int customerId, DateTime fromDate)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return;

                var subsequentBills = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .Where(e => e.Customer_ID == customerId &&
                               e.Date > fromDate &&
                               e.Customer.UserId == currentUserId.Value)
                    .OrderBy(e => e.Date)
                    .ToListAsync();

                foreach (var bill in subsequentBills)
                {
                    // Find the bill immediately before this one
                    var previousBill = await _context.ElectricBills
                        .Include(e => e.Customer)
                        .Where(e => e.Customer_ID == customerId &&
                                   e.Date < bill.Date &&
                                   e.ID != bill.ID &&
                                   e.Customer.UserId == currentUserId.Value)
                        .OrderByDescending(e => e.Date)
                        .FirstOrDefaultAsync();

                    decimal previousUnit;
                    decimal previousDues;

                    if (previousBill == null)
                    {
                        // Get from customer table
                        var customer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.C_ID == customerId && c.UserId == currentUserId.Value);
                        previousUnit = customer?.Previous_Unit ?? 0;
                        previousDues = customer?.Advance_money ?? 0;
                    }
                    else
                    {
                        previousUnit = previousBill.Current_Unit;
                        previousDues = previousBill.Present_dues;
                    }

                    // Recalculate using corrected business logic
                    decimal totalUnit = bill.Current_Unit - previousUnit;
                    decimal electricBillAmount = totalUnit * RATE_PER_UNIT;
                    decimal totalBill = previousDues + electricBillAmount + bill.Rent_Bill;
                    decimal presentDues = totalBill - bill.Clear_money;

                    // Update
                    bill.Previous_unit = previousUnit;
                    bill.Total_Unit = totalUnit;
                    bill.Previous_duos = previousDues;
                    bill.Electric_bill = electricBillAmount;
                    bill.Total_bill = totalBill;
                    bill.Present_dues = presentDues;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating subsequent bills: {ex.Message}");
            }
        }

        public async Task<bool> DeleteElectricBillAsync(int id)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return false;

                var electricBillEntity = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.ID == id && e.Customer.UserId == currentUserId.Value);

                if (electricBillEntity == null) return false;

                int customerId = electricBillEntity.Customer_ID;
                DateTime billDate = electricBillEntity.Date;

                _context.ElectricBills.Remove(electricBillEntity);
                await _context.SaveChangesAsync();

                // Update subsequent bills
                await UpdateSubsequentBillsAsync(customerId, billDate.AddDays(-1));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting electric bill: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return new List<Customer>();

                return await _context.Customers
                    .Where(c => c.UserId == currentUserId.Value)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        // Helper method to get customer's last bill summary
        public async Task<CustomerBillSummaryDTO> GetCustomerBillSummaryAsync(int customerId)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return null;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == customerId && c.UserId == currentUserId.Value);

                if (customer == null) return null;

                var lastBill = await _context.ElectricBills
                    .Where(e => e.Customer_ID == customerId && e.Customer.UserId == currentUserId.Value)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefaultAsync();

                if (lastBill == null)
                {
                    // No previous bills, return initial values from customer
                    return new CustomerBillSummaryDTO
                    {
                        CustomerId = customerId,
                        CustomerName = customer.Name,
                        LastMeterReading = customer.Previous_Unit,
                        PreviousDues = customer.Advance_money,
                        LastBillDate = null
                    };
                }

                return new CustomerBillSummaryDTO
                {
                    CustomerId = customerId,
                    CustomerName = customer.Name,
                    LastMeterReading = lastBill.Current_Unit,
                    PreviousDues = lastBill.Present_dues,
                    LastBillDate = lastBill.Date
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer bill summary: {ex.Message}");
                return null;
            }
        }

        // Method to preview bill calculation before saving
        public async Task<ElectricBillPreviewDTO> PreviewElectricBillAsync(int customerId, decimal currentMeterReading, decimal rentBill)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return null;

                var summary = await GetCustomerBillSummaryAsync(customerId);
                if (summary == null) return null;

                // CORRECTED BUSINESS LOGIC FOR PREVIEW:
                decimal totalUnit = currentMeterReading - summary.LastMeterReading;
                decimal electricBillAmount = totalUnit * RATE_PER_UNIT;
                decimal totalBill = summary.PreviousDues + electricBillAmount + rentBill;

                return new ElectricBillPreviewDTO
                {
                    CustomerId = customerId,
                    CustomerName = summary.CustomerName,
                    PreviousMeterReading = summary.LastMeterReading,
                    CurrentMeterReading = currentMeterReading,
                    ConsumedUnits = totalUnit,
                    ElectricBill = electricBillAmount,
                    RentBill = rentBill,
                    PreviousDues = summary.PreviousDues,
                    TotalBill = totalBill
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PreviewElectricBillAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<ElectricBillDTO> GetLatestElectricBillByCustomerIdAsync(int customerId)
        {
            try
            {
                var currentUserId = await GetCurrentUserIdAsync();
                if (!currentUserId.HasValue)
                    return null;

                var electricBill = await _context.ElectricBills
                    .Include(e => e.Customer)
                    .Where(e => e.Customer_ID == customerId && e.Customer.UserId == currentUserId.Value)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefaultAsync();

                if (electricBill == null) return null;

                return new ElectricBillDTO
                {
                    ID = electricBill.ID,
                    Customer_ID = electricBill.Customer_ID,
                    Date = electricBill.Date,
                    Previous_unit = electricBill.Previous_unit,
                    Current_Unit = electricBill.Current_Unit,
                    Total_Unit = electricBill.Total_Unit,
                    Electric_bill = electricBill.Electric_bill,
                    Previous_duos = electricBill.Previous_duos,
                    Rent_Bill = electricBill.Rent_Bill,
                    Total_bill = electricBill.Total_bill,
                    Clear_money = electricBill.Clear_money,
                    Present_dues = electricBill.Present_dues,
                    CustomerName = electricBill.Customer.Name
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLatestElectricBillByCustomerIdAsync: {ex.Message}");
                return null;
            }
        }
    }
}