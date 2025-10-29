using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.IdentityService.DTOs;
using OnlineAuctionSystem.IdentityService.Helpers;
using OnlineAuctionSystem.IdentityService.Models;
// Add these using statements at the top
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace OnlineAuctionSystem.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AccountController(UserManager<ApplicationUser> userManager, JwtTokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // Simple validation for roles matching the auction system simulation
            if (model.Role != "Buyer" && model.Role != "Seller" && model.Role != "Admin")
            {
                return BadRequest(new { message = "Invalid role specified. Must be 'Buyer', 'Seller', or 'Admin'." });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                Role = model.Role
                // FullName will be added in the next step
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "User registered successfully." });
            }

            return BadRequest(new { message = "User registration failed.", errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Successful login: Generate JWT Token
                var token = _tokenGenerator.GenerateToken(user);

                // Return the token to the React frontend
                return Ok(new
                {
                    token,
                    username = user.UserName,
                    email = user.Email,
                    role = user.Role // Send back key user info for initial context
                });
            }

            return Unauthorized(new { message = "Invalid credentials." });
        }

        // ... inside the AccountController class ...

        [HttpGet("users")]
        [Authorize(Roles = "Admin")] // Secure this endpoint
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new { u.Id, u.UserName, u.Email, u.Role, u.FullName })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")] // Secure this endpoint
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully." });
            }
            return BadRequest(new { message = "Failed to delete user." });
        }
    }
}