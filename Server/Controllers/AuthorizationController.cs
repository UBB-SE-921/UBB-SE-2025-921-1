using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Service;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly SharedClassLibrary.Service.IAuthorizationService authorizationService; // Fully qualified name to avoid conflicts

        public AuthorizationController(IUserRepository userRepository, SharedClassLibrary.Service.IAuthorizationService authorizationService)
        {
            this.userRepository = userRepository;
            this.authorizationService = authorizationService;
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            // Generate JWT token
            var token = this.authorizationService.GenerateJwtToken();

            // Set JWT in secure, HttpOnly cookie
            this.Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1),
            });

            Debug.WriteLine("JwtToken created: " + token);

            return this.Ok(new { token = token, message = "Login successful" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            this.Response.Cookies.Delete("access_token");

            return this.Ok(new { message = "Logged out successfully" });
        }
    }
}