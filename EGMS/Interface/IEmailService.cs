namespace EGMS.Interface
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string name, string resetLink);
    }
}
