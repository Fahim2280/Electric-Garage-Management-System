using EGMS.DTOs;
using EGMS.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace EGMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
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
                Console.WriteLine($"SignIn error: {ex.Message}");
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
    }
}