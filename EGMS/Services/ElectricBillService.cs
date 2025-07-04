using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EGMS.Services
{
    public class ElectricBillService : IElectricBillService
    {
        private readonly AppDbContext _context;
        private const decimal RATE_PER_UNIT = 15; // 15 taka per unit

        public ElectricBillService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ElectricBillDTO>> GetAllElectricBillsAsync()
        {
            var electricBills = await _context.ElectricBills
                .Include(e => e.Customer)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return electricBills.Select(e => new ElectricBillDTO
            {
                ID = e.ID,
                Customer_ID = e.Customer_ID,
                Date = e.Date,
                Previous_unit = e.Previous_unit,
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

        public async Task<ElectricBillDTO> GetElectricBillByIdAsync(int id)
        {
            var electricBill = await _context.ElectricBills
                .Include(e => e.Customer)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (electricBill == null) return null;

            return new ElectricBillDTO
            {
                ID = electricBill.ID,
                Customer_ID = electricBill.Customer_ID,
                Date = electricBill.Date,
                Previous_unit = electricBill.Previous_unit,
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

        public async Task<IEnumerable<ElectricBillDTO>> GetElectricBillsByCustomerIdAsync(int customerId)
        {
            var electricBills = await _context.ElectricBills
                .Include(e => e.Customer)
                .Where(e => e.Customer_ID == customerId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return electricBills.Select(e => new ElectricBillDTO
            {
                ID = e.ID,
                Customer_ID = e.Customer_ID,
                Date = e.Date,
                Previous_unit = e.Previous_unit,
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

        public async Task<ElectricBillDTO> GetLatestElectricBillByCustomerIdAsync(int customerId)
        {
            var electricBill = await _context.ElectricBills
                .Include(e => e.Customer)
                .Where(e => e.Customer_ID == customerId)
                .OrderByDescending(e => e.Date)
                .FirstOrDefaultAsync();

            if (electricBill == null) return null;

            return new ElectricBillDTO
            {
                ID = electricBill.ID,
                Customer_ID = electricBill.Customer_ID,
                Date = electricBill.Date,
                Previous_unit = electricBill.Previous_unit,
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

        // Helper method to get the correct previous unit for a specific date
        private async Task<decimal> GetPreviousUnitForDate(int customerId, DateTime billDate, int? excludeBillId = null)
        {
            var previousBill = await _context.ElectricBills
                .Where(e => e.Customer_ID == customerId &&
                           e.Date < billDate &&
                           (excludeBillId == null || e.ID != excludeBillId))
                .OrderByDescending(e => e.Date)
                .FirstOrDefaultAsync();

            if (previousBill != null)
            {
                Console.WriteLine($"DEBUG: Found previous bill for customer {customerId}, Total_Unit: {previousBill.Total_Unit}");
                return previousBill.Total_Unit;
            }

            // No previous bill found, get from customer's initial reading
            var customer = await _context.Customers.FindAsync(customerId);
            Console.WriteLine($"DEBUG: Customer {customerId} found: {customer != null}");

            if (customer != null)
            {
                Console.WriteLine($"DEBUG: Customer.Previous_Unit raw value: '{customer.Previous_Unit}'");
                var parseResult = decimal.TryParse(customer.Previous_Unit, out var unit);
                Console.WriteLine($"DEBUG: Parse successful: {parseResult}, Value: {unit}");
                return parseResult ? unit : 0;
            }

            Console.WriteLine($"DEBUG: Customer {customerId} not found, returning 0");
            return 0;
        }

        // Helper method to get the correct previous dues for a specific date
        private async Task<decimal> GetPreviousDuesForDate(int customerId, DateTime billDate, int? excludeBillId = null)
        {
            var previousBill = await _context.ElectricBills
                .Where(e => e.Customer_ID == customerId &&
                           e.Date < billDate &&
                           (excludeBillId == null || e.ID != excludeBillId))
                .OrderByDescending(e => e.Date)
                .FirstOrDefaultAsync();

            if (previousBill != null)
            {
                return previousBill.Present_dues;
            }

            // No previous bill found, get from customer's advance money
            var customer = await _context.Customers.FindAsync(customerId);
            return decimal.TryParse(customer?.Advance_money, out var advance) ? advance : 0;
        }

        public async Task<bool> CreateElectricBillAsync(ElectricBillDTO electricBillDto)
        {
            try
            {
                // Get customer information
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == electricBillDto.Customer_ID);

                if (customer == null)
                    return false;

                // Get the correct previous unit and dues for this bill's date
                decimal previousUnit = await GetPreviousUnitForDate(electricBillDto.Customer_ID, electricBillDto.Date);
                decimal previousDues = await GetPreviousDuesForDate(electricBillDto.Customer_ID, electricBillDto.Date);

                // AUTOMATIC CALCULATIONS:
                decimal consumedUnits = electricBillDto.Total_Unit - previousUnit;
                decimal electricBillAmount = consumedUnits * RATE_PER_UNIT;
                decimal totalBill = electricBillAmount + electricBillDto.Rent_Bill + previousDues;
                decimal presentDues = totalBill - electricBillDto.Clear_money;

                var electricBillEntity = new ElectricBill
                {
                    Customer_ID = electricBillDto.Customer_ID,
                    Date = electricBillDto.Date,
                    Previous_unit = previousUnit,
                    Total_Unit = electricBillDto.Total_Unit,
                    Electric_bill = electricBillAmount,
                    Previous_duos = previousDues,
                    Rent_Bill = electricBillDto.Rent_Bill,
                    Total_bill = totalBill,
                    Clear_money = electricBillDto.Clear_money,
                    Present_dues = presentDues
                };

                _context.ElectricBills.Add(electricBillEntity);
                await _context.SaveChangesAsync();

                // Update all subsequent bills for this customer
                await UpdateSubsequentBillsAsync(electricBillDto.Customer_ID, electricBillDto.Date);

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
                var electricBillEntity = await _context.ElectricBills.FindAsync(electricBillDto.ID);
                if (electricBillEntity == null) return false;

                // Get customer information
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.C_ID == electricBillDto.Customer_ID);

                if (customer == null) return false;

                // Get the correct previous unit and dues for this bill's date (excluding current bill)
                decimal previousUnit = await GetPreviousUnitForDate(electricBillDto.Customer_ID, electricBillDto.Date, electricBillDto.ID);
                decimal previousDues = await GetPreviousDuesForDate(electricBillDto.Customer_ID, electricBillDto.Date, electricBillDto.ID);

                // AUTOMATIC CALCULATIONS:
                decimal consumedUnits = electricBillDto.Total_Unit - previousUnit;
                decimal electricBillAmount = consumedUnits * RATE_PER_UNIT;
                decimal totalBill = electricBillAmount + electricBillDto.Rent_Bill + previousDues;
                decimal presentDues = totalBill - electricBillDto.Clear_money;

                // Update the entity with calculated values
                electricBillEntity.Customer_ID = electricBillDto.Customer_ID;
                electricBillEntity.Date = electricBillDto.Date;
                electricBillEntity.Previous_unit = previousUnit;
                electricBillEntity.Total_Unit = electricBillDto.Total_Unit;
                electricBillEntity.Electric_bill = electricBillAmount;
                electricBillEntity.Previous_duos = previousDues;
                electricBillEntity.Rent_Bill = electricBillDto.Rent_Bill;
                electricBillEntity.Total_bill = totalBill;
                electricBillEntity.Clear_money = electricBillDto.Clear_money;
                electricBillEntity.Present_dues = presentDues;

                await _context.SaveChangesAsync();

                // Update all subsequent bills for this customer
                await UpdateSubsequentBillsAsync(electricBillDto.Customer_ID, electricBillDto.Date);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating electric bill: {ex.Message}");
                return false;
            }
        }

        // Method to update all subsequent bills when a bill is created/updated
        private async Task UpdateSubsequentBillsAsync(int customerId, DateTime fromDate)
        {
            var subsequentBills = await _context.ElectricBills
                .Where(e => e.Customer_ID == customerId && e.Date > fromDate)
                .OrderBy(e => e.Date)
                .ToListAsync();

            foreach (var bill in subsequentBills)
            {
                // Recalculate previous unit and dues for this bill
                decimal previousUnit = await GetPreviousUnitForDate(customerId, bill.Date, bill.ID);
                decimal previousDues = await GetPreviousDuesForDate(customerId, bill.Date, bill.ID);

                // Recalculate all values
                decimal consumedUnits = bill.Total_Unit - previousUnit;
                decimal electricBillAmount = consumedUnits * RATE_PER_UNIT;
                decimal totalBill = electricBillAmount + bill.Rent_Bill + previousDues;
                decimal presentDues = totalBill - bill.Clear_money;

                // Update the bill
                bill.Previous_unit = previousUnit;
                bill.Previous_duos = previousDues;
                bill.Electric_bill = electricBillAmount;
                bill.Total_bill = totalBill;
                bill.Present_dues = presentDues;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteElectricBillAsync(int id)
        {
            try
            {
                var electricBillEntity = await _context.ElectricBills.FindAsync(id);
                if (electricBillEntity == null) return false;

                int customerId = electricBillEntity.Customer_ID;
                DateTime billDate = electricBillEntity.Date;

                _context.ElectricBills.Remove(electricBillEntity);
                await _context.SaveChangesAsync();

                // Update all subsequent bills for this customer
                await UpdateSubsequentBillsAsync(customerId, billDate.AddDays(-1));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.OrderBy(c => c.Name).ToListAsync();
        }

        // Helper method to get customer's last bill summary
        public async Task<CustomerBillSummaryDTO> GetCustomerBillSummaryAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return null;

            var lastBill = await _context.ElectricBills
                .Where(e => e.Customer_ID == customerId)
                .OrderByDescending(e => e.Date)
                .FirstOrDefaultAsync();

            if (lastBill == null)
            {
                // No previous bills, return initial values from customer
                return new CustomerBillSummaryDTO
                {
                    CustomerId = customerId,
                    CustomerName = customer.Name,
                    LastMeterReading = decimal.TryParse(customer.Previous_Unit, out var unit) ? unit : 0,
                    PreviousDues = decimal.TryParse(customer.Advance_money, out var advance) ? advance : 0,
                    LastBillDate = null
                };
            }

            return new CustomerBillSummaryDTO
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                LastMeterReading = lastBill.Total_Unit,
                PreviousDues = lastBill.Present_dues,
                LastBillDate = lastBill.Date
            };
        }

        // Method to preview bill calculation before saving
        public async Task<ElectricBillPreviewDTO> PreviewElectricBillAsync(int customerId, decimal currentMeterReading, decimal rentBill)
        {
            var summary = await GetCustomerBillSummaryAsync(customerId);
            if (summary == null) return null;

            decimal consumedUnits = currentMeterReading - summary.LastMeterReading;
            decimal electricBillAmount = consumedUnits * RATE_PER_UNIT;
            decimal totalBill = electricBillAmount + rentBill + summary.PreviousDues;

            return new ElectricBillPreviewDTO
            {
                CustomerId = customerId,
                CustomerName = summary.CustomerName,
                PreviousMeterReading = summary.LastMeterReading,
                CurrentMeterReading = currentMeterReading,
                ConsumedUnits = consumedUnits,
                ElectricBill = electricBillAmount,
                RentBill = rentBill,
                PreviousDues = summary.PreviousDues,
                TotalBill = totalBill
            };
        }
    }
}