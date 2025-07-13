using EGMS.Interface;

namespace EGMS.Services
{
    // Temporary mock email service for testing
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string userName, string resetLink)
        {
            _logger.LogInformation($"🔥 MOCK EMAIL SERVICE 🔥");
            _logger.LogInformation($"📧 TO: {email}");
            _logger.LogInformation($"👤 USER: {userName}");
            _logger.LogInformation($"🔗 RESET LINK: {resetLink}");
            _logger.LogInformation($"");
            _logger.LogInformation($"=== EMAIL CONTENT ===");
            _logger.LogInformation($"Subject: Password Reset Request");
            _logger.LogInformation($"Dear {userName},");
            _logger.LogInformation($"");
            _logger.LogInformation($"You have requested to reset your password. Please click the following link:");
            _logger.LogInformation($"{resetLink}");
            _logger.LogInformation($"");
            _logger.LogInformation($"This link will expire in 24 hours.");
            _logger.LogInformation($"If you did not request this, please ignore this email.");
            _logger.LogInformation($"=== END EMAIL ===");

            // Simulate async email sending
            await Task.Delay(100);

            _logger.LogInformation($"✅ Mock email 'sent' successfully!");
        }

        // Add other email methods as needed
        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            _logger.LogInformation($"📧 Mock Welcome Email to: {email} for user: {userName}");
            await Task.Delay(50);
        }

        Task<bool> IEmailService.SendPasswordResetEmailAsync(string email, string name, string resetLink)
        {
            throw new NotImplementedException();
        }
    }
}

// Interface definition (if you don't have it)
