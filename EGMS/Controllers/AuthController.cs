using EGMS.DTOs;
using EGMS.Interface;
using Microsoft.AspNetCore.Mvc;

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
            return View(new UserLoginDTO()); // Return empty DTO, not User model
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
                var token = await _userService.Login(dto);

                if (token == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(dto);
                }

                // Store JWT in cookie
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Use HTTPS in production
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(3)
                });

                return RedirectToAction("Index", "Home"); // Specify controller
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
        public IActionResult SignOut()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("SignIn");
        }
    }
}