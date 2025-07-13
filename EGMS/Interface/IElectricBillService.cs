using EGMS.DTOs;
using EGMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EGMS.Interface
{
    public interface IElectricBillService
    {
        Task<IEnumerable<ElectricBillDTO>> GetAllElectricBillsAsync();
        Task<ElectricBillDTO> GetElectricBillByIdAsync(int id);
        Task<IEnumerable<ElectricBillDTO>> GetElectricBillsByCustomerIdAsync(int customerId);
        Task<bool> CreateElectricBillAsync(ElectricBillDTO electricBillDto);
        Task<bool> UpdateElectricBillAsync(ElectricBillDTO electricBillDto);
        Task<bool> DeleteElectricBillAsync(int id);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<CustomerBillSummaryDTO> GetCustomerBillSummaryAsync(int customerId);
        //Task PreviewElectricBillAsync(int customerId, decimal currentMeterReading, decimal rentBill);
        Task<ElectricBillPreviewDTO> PreviewElectricBillAsync(int customerId, decimal currentMeterReading, decimal rentBill);
        Task<ElectricBillDTO> GetLatestElectricBillByCustomerIdAsync(int customerId);
    }
}