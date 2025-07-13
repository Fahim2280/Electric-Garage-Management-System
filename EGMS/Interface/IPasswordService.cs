using EGMS.DTOs;

namespace EGMS.Interface
{
    public interface IPasswordService
    {
        Task<bool> ChangePasswordAsync(string username, ChangePasswordDTO dto);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
        Task<bool> ValidateResetTokenAsync(string token, string email);
    }
}