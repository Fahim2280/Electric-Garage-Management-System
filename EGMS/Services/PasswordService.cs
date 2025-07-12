using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EGMS.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordService> _logger;
        private readonly IConfiguration _configuration;

        public PasswordService(AppDbContext context, IEmailService emailService, ILogger<PasswordService> logger, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> ChangePasswordAsync(string username, ChangePasswordDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {username}");
                    return false;
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning($"Invalid current password for user: {username}");
                    return false;
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, 12);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed successfully for user: {username}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user: {username}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"=== STARTING PASSWORD RESET PROCESS ===");
                _logger.LogInformation($"Email received: '{email}'");

                var normalizedEmail = email.Trim().ToLower();
                _logger.LogInformation($"Normalized email: '{normalizedEmail}'");

                // Check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

                if (user == null)
                {
                    _logger.LogWarning($"NO USER FOUND for email: {email}");
                    // List all users for debugging (remove in production)
                    var allUsers = await _context.Users.Select(u => new { u.Email, u.Username }).ToListAsync();
                    _logger.LogInformation($"Available users in database: {string.Join(", ", allUsers.Select(u => $"{u.Username}({u.Email})"))}");

                    // Still return true for security - don't reveal if email exists
                    return true;
                }

                _logger.LogInformation($"✓ User found: {user.Username} with email: {user.Email}");

                // Clean up expired tokens for this email
                var expiredTokens = await _context.PasswordResetTokens
                    .Where(t => t.Email == normalizedEmail && (t.ExpiryDate < DateTime.UtcNow || t.IsUsed))
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.PasswordResetTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✓ Cleaned up {expiredTokens.Count} expired tokens");
                }

                // Generate reset token
                var token = GenerateResetToken();
                var expiryDate = DateTime.UtcNow.AddHours(24);

                _logger.LogInformation($"✓ Generated token: {token.Substring(0, Math.Min(10, token.Length))}...");
                _logger.LogInformation($"✓ Token expires at: {expiryDate}");

                // Save token to database
                var resetToken = new PasswordResetToken
                {
                    Email = normalizedEmail,
                    Token = token,
                    ExpiryDate = expiryDate,
                    IsUsed = false,
                    CreatedDate = DateTime.UtcNow
                };

                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Token saved to database with ID: {resetToken.Id}");

                // Create reset link
                var baseUrl = _configuration["AppSettings:BaseUrl"] ??
                             _configuration["BaseUrl"] ??
                             "https://localhost:7000"; // fallback

                var resetLink = $"{baseUrl}/Auth/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

                _logger.LogInformation($"✓ Generated reset link: {resetLink}");

                // FOR DEBUGGING: Log the direct test link
                _logger.LogInformation($"🔗 DIRECT TEST LINK: {resetLink}");
                _logger.LogInformation($"🔗 You can copy this link and paste it directly in your browser to test");

                // Try to send email
                try
                {
                    if (_emailService == null)
                    {
                        _logger.LogError("❌ EMAIL SERVICE IS NULL - Email will not be sent!");

                        // FOR DEVELOPMENT: Return false to indicate email wasn't sent
                        // In production, you might want to return true for security
                        return false;
                    }

                    await _emailService.SendPasswordResetEmailAsync(email, user.Name, resetLink);
                    _logger.LogInformation($"✓ Email sent successfully to: {email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"❌ Failed to send email to: {email}");
                    _logger.LogError($"Email service error: {emailEx.Message}");

                    // FOR DEVELOPMENT: Return false to indicate email wasn't sent
                    return false;
                }

                _logger.LogInformation($"=== PASSWORD RESET PROCESS COMPLETED ===");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ CRITICAL ERROR in SendPasswordResetEmailAsync for: {email}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            try
            {
                _logger.LogInformation($"=== STARTING PASSWORD RESET VALIDATION ===");
                _logger.LogInformation($"Email: {dto.Email}");
                _logger.LogInformation($"Token: {dto.Token?.Substring(0, Math.Min(10, dto.Token?.Length ?? 0))}...");

                var normalizedEmail = dto.Email.Trim().ToLower();

                var resetToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(rt => rt.Token == dto.Token &&
                                             rt.Email == normalizedEmail &&
                                             !rt.IsUsed &&
                                             rt.ExpiryDate > DateTime.UtcNow);

                if (resetToken == null)
                {
                    _logger.LogWarning($"❌ INVALID OR EXPIRED TOKEN for email: {dto.Email}");

                    // Comprehensive debugging
                    var allTokens = await _context.PasswordResetTokens
                        .Where(rt => rt.Email == normalizedEmail)
                        .OrderByDescending(rt => rt.CreatedDate)
                        .ToListAsync();

                    _logger.LogInformation($"📊 Found {allTokens.Count} total tokens for email {dto.Email}");

                    for (int i = 0; i < allTokens.Count; i++)
                    {
                        var t = allTokens[i];
                        var tokenMatch = t.Token == dto.Token;
                        var emailMatch = t.Email == normalizedEmail;
                        var notUsed = !t.IsUsed;
                        var notExpired = t.ExpiryDate > DateTime.UtcNow;

                        _logger.LogInformation($"Token {i + 1}: " +
                            $"TokenMatch: {tokenMatch}, " +
                            $"EmailMatch: {emailMatch}, " +
                            $"NotUsed: {notUsed}, " +
                            $"NotExpired: {notExpired}, " +
                            $"Created: {t.CreatedDate}, " +
                            $"Expires: {t.ExpiryDate}");
                    }

                    return false;
                }

                _logger.LogInformation($"✓ Valid token found for email: {dto.Email}");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
                if (user == null)
                {
                    _logger.LogWarning($"❌ User not found for email: {dto.Email}");
                    return false;
                }

                _logger.LogInformation($"✓ User found: {user.Username}");

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, 12);

                // Mark token as used
                resetToken.IsUsed = true;

                _context.Users.Update(user);
                _context.PasswordResetTokens.Update(resetToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Password reset completed successfully for: {dto.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error resetting password for email: {dto.Email}");
                return false;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string token, string email)
        {
            try
            {
                _logger.LogInformation($"=== VALIDATING RESET TOKEN ===");
                _logger.LogInformation($"Email: {email}");
                _logger.LogInformation($"Token: {token?.Substring(0, Math.Min(10, token?.Length ?? 0))}...");
                _logger.LogInformation($"Current UTC time: {DateTime.UtcNow}");

                var normalizedEmail = email.Trim().ToLower();

                var resetToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token &&
                                             rt.Email == normalizedEmail &&
                                             !rt.IsUsed &&
                                             rt.ExpiryDate > DateTime.UtcNow);

                if (resetToken == null)
                {
                    _logger.LogWarning($"❌ TOKEN VALIDATION FAILED for email: {email}");

                    // Detailed debugging
                    var allTokensForEmail = await _context.PasswordResetTokens
                        .Where(rt => rt.Email == normalizedEmail)
                        .OrderByDescending(rt => rt.CreatedDate)
                        .ToListAsync();

                    _logger.LogInformation($"📊 Total tokens for {email}: {allTokensForEmail.Count}");

                    for (int i = 0; i < allTokensForEmail.Count; i++)
                    {
                        var t = allTokensForEmail[i];
                        var tokenMatch = t.Token == token;
                        var emailMatch = t.Email == normalizedEmail;
                        var notUsed = !t.IsUsed;
                        var notExpired = t.ExpiryDate > DateTime.UtcNow;

                        _logger.LogInformation($"Token {i + 1}: " +
                            $"Match: {tokenMatch}, " +
                            $"Email: {emailMatch}, " +
                            $"NotUsed: {notUsed}, " +
                            $"NotExpired: {notExpired} " +
                            $"(Expires: {t.ExpiryDate})");
                    }

                    return false;
                }

                _logger.LogInformation($"✓ TOKEN VALIDATION SUCCESSFUL for email: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error validating reset token for email: {email}");
                return false;
            }
        }

        private string GenerateResetToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .TrimEnd('=');
            }
        }
    }
}