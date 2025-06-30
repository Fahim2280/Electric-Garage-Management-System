using EGMS.DTOs;
using EGMS.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.ModelBinding;

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
        public IActionResult SignUp() => View();

        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterDTO dto)
        {
            var result = await _userService.Register(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(dto);
            }

            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginDTO dto)
        {
            var token = await _userService.Login(dto);
            if (token == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(dto);
            }

            // Store JWT in cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(3)
            });

            return RedirectToAction("Index", "Dashboard");
        }
    }

}
