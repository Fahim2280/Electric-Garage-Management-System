using EGMS.DTOs;
using EGMS.Interface;
using EGMS.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace EGMS.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<bool> Register(UserRegisterDTO dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                    return false;

                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                    return false;

                // Create new user with properly hashed password
                var user = new User
                {
                    Username = dto.Username.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12), // Use work factor instead of GenerateSalt
                    Role = dto.Role ?? "Admin" // Default role if not provided
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<string> Login(UserLoginDTO dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                    return null;

                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username.Trim());

                // Check if user exists
                if (user == null)
                {
                    Console.WriteLine($"User not found: {dto.Username}");
                    return null;
                }

                // Check if password hash exists
                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    Console.WriteLine($"Password hash is null or empty for user: {dto.Username}");
                    return null;
                }

                // Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    Console.WriteLine($"Password verification failed for user: {dto.Username}");
                    return null;
                }

                // Generate JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role ?? "Admin")
                    }),
                    Expires = DateTime.UtcNow.AddHours(3),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}