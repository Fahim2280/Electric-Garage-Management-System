using EGMS.Interface;
using System.Net.Mail;
using System.Net;

namespace EGMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string name, string resetLink)
        {
            try
            {
                var smtpSettings = _config.GetSection("EmailSettings");
                var smtpServer = smtpSettings["SmtpServer"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"]);
                var smtpUsername = smtpSettings["SmtpUsername"];
                var smtpPassword = smtpSettings["SmtpPassword"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                var subject = "Reset Your Password - EGMS";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #007bff; border-bottom: 2px solid #007bff; padding-bottom: 10px;'>
                                Password Reset Request
                            </h2>
                            
                            <p>Dear {name},</p>
                            
                            <p>We received a request to reset your password for your EGMS account.</p>
                            
                            <p>Click the button below to reset your password:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' 
                                   style='background-color: #007bff; color: white; padding: 12px 30px; 
                                          text-decoration: none; border-radius: 5px; font-weight: bold;
                                          display: inline-block;'>
                                    Reset Password
                                </a>
                            </div>
                            
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all;'>
                                {resetLink}
                            </p>
                            
                            <p><strong>Important:</strong> This link will expire in 1 hour for security reasons.</p>
                            
                            <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
                            
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            
                            <p style='color: #666; font-size: 12px;'>
                                This is an automated message from EGMS. Please do not reply to this email.
                            </p>
                        </div>
                    </body>
                    </html>";

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true;

                    var message = new MailMessage
                    {
                        From = new MailAddress(fromEmail, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    message.To.Add(email);

                    await client.SendMailAsync(message);
                }

                _logger.LogInformation($"Password reset email sent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to: {email}");
                return false;
            }
        }
    }
}
