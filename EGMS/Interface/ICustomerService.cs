using EGMS.DTOs;

namespace EGMS.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
        Task<CustomerDTO> GetCustomerByIdAsync(int id);
        Task<bool> CreateCustomerAsync(CustomerCreateDTO dto);
        Task<bool> UpdateCustomerAsync(int id, CustomerDTO dto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> CustomerExistsAsync(int id);
        Task<bool> IsNIDUniqueAsync(string nid, int? excludeId = null);
        Task<bool> IsMobileUniqueAsync(string mobile, int? excludeId = null);
    }
}
