using EGMS.DTOs;
using EGMS.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EGMS.Services;

namespace EGMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, AppDbContext context, IPasswordService passwordService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get the current user's username from claims
                var username = User.Identity.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("SignIn", "Auth");
                }

                // Find the user in the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return RedirectToAction("SignIn", "Auth");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Profile error");
                TempData["ErrorMessage"] = "An error occurred while loading your profile.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View(new UserRegisterDTO());
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _userService.Register(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Username already exists or registration failed.");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Registration successful! Please sign in.";
            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            return View(new UserLoginDTO());
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                // Verify user credentials through your service
                var token = await _userService.Login(dto);
                if (token == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(dto);
                }

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dto.Username),
                    new Claim(ClaimTypes.NameIdentifier, dto.Username),
                    // Add more claims as needed
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user using cookie authentication
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "SignIn error");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var username = User.Identity.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("SignIn", "Auth");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return RedirectToAction("SignIn", "Auth");
                }

                var editDto = new UserEditDTO
                {
                    Name = user.Name,
                    Email = user.Email,
                    Number = user.Number,
                    CompanyName = user.CompanyName
                };

                return View(editDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile edit error");
                TempData["ErrorMessage"] = "An error occurred while loading your profile for editing.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var username = User.Identity.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("SignIn", "Auth");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return RedirectToAction("SignIn", "Auth");
                }

                // Check if email is being changed and if it's already taken by another user
                if (user.Email != dto.Email.Trim().ToLower())
                {
                    var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email.Trim().ToLower() && u.Id != user.Id);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "This email is already registered by another user.");
                        return View(dto);
                    }
                }

                // Update user information
                user.Name = dto.Name.Trim();
                user.Email = dto.Email.Trim().ToLower();
                user.Number = dto.Number.Trim();
                user.CompanyName = dto.CompanyName.Trim();

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile update error");
                ModelState.AddModelError("", "An error occurred while updating your profile. Please try again.");
                return View(dto);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                _logger.LogInformation($"Processing password reset request for email: {dto.Email}");

                var result = await _passwordService.SendPasswordResetEmailAsync(dto.Email);

                if (result)
                {
                    _logger.LogInformation($"Password reset email process completed for: {dto.Email}");
                    TempData["SuccessMessage"] = "If an account with that email exists, we've sent you a password reset link.";
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                else
                {
                    _logger.LogWarning($"Password reset email failed for: {dto.Email}");
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again.");
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ForgotPassword error for email: {dto.Email}");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again.");
                return View(dto);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            _logger.LogInformation($"ResetPassword GET called with token: {token?.Substring(0, Math.Min(10, token?.Length ?? 0))}... and email: {email}");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("ResetPassword: Missing token or email");
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("SignIn");
            }

            try
            {
                var isValid = await _passwordService.ValidateResetTokenAsync(token, email);
                if (!isValid)
                {
                    _logger.LogWarning($"ResetPassword: Invalid token for email {email}");
                    TempData["ErrorMessage"] = "This password reset link is invalid or has expired.";
                    return RedirectToAction("SignIn");
                }

                var model = new ResetPasswordDTO
                {
                    Token = token,
                    Email = email
                };

                _logger.LogInformation($"ResetPassword: Valid token for email {email}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ResetPassword validation error for email: {email}");
                TempData["ErrorMessage"] = "An error occurred while validating the reset link.";
                return RedirectToAction("SignIn");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                _logger.LogInformation($"Processing password reset for email: {dto.Email}");

                var result = await _passwordService.ResetPasswordAsync(dto);

                if (result)
                {
                    _logger.LogInformation($"Password reset successful for email: {dto.Email}");
                    TempData["SuccessMessage"] = "Your password has been reset successfully. Please sign in with your new password.";
                    return RedirectToAction("SignIn");
                }
                else
                {
                    _logger.LogWarning($"Password reset failed for email: {dto.Email}");
                    ModelState.AddModelError("", "Failed to reset password. The link may have expired or been used already.");
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ResetPassword error for email: {dto.Email}");
                ModelState.AddModelError("", "An error occurred while resetting your password. Please try again.");
                return View(dto);
            }
        }
    }
}