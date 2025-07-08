using EGMS.DTOs;

namespace EGMS.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
        Task<CustomerDTO?> GetCustomerByIdAsync(int id);
        Task<bool> CreateCustomerAsync(CustomerCreateDTO dto);
        Task<bool> UpdateCustomerAsync(int id, CustomerDTO dto);
        Task<IEnumerable<ElectricBillDTO>> GetCustomerBillsAsync(int customerId);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> IsNIDUniqueAsync(string nidNumber, int? excludeCustomerId = null);
        Task<bool> IsMobileUniqueAsync(string mobileNumber, int? excludeCustomerId = null);
    }
}