using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using OnlineAuctionSystem.IdentityService.DTOs;
using OnlineAuctionSystem.IdentityService.Helpers;
using OnlineAuctionSystem.IdentityService.Models;
using System.Security.Claims;

namespace OnlineAuctionSystem.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            JwtTokenGenerator tokenGenerator,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            _logger.LogInformation("Registration attempt for user: {Username}", model.Username);

            if (model.Role != "Buyer" && model.Role != "Seller" && model.Role != "Admin")
            {
                _logger.LogWarning("Invalid role specified: {Role}", model.Role);
                return BadRequest(new { message = "Invalid role specified. Must be 'Buyer', 'Seller', or 'Admin'." });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User registered successfully: {Username}", model.Username);
                return Ok(new { message = "User registered successfully." });
            }

            _logger.LogError("User registration failed for {Username}: {Errors}", model.Username, result.Errors);
            return BadRequest(new
            {
                message = "User registration failed.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            _logger.LogInformation("Login attempt for user: {Username}", model.Username);

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = _tokenGenerator.GenerateToken(user);

                _logger.LogInformation("Login successful for user: {Username}", model.Username);
                return Ok(new
                {
                    token,
                    username = user.UserName,
                    email = user.Email,
                    role = user.Role
                });
            }

            _logger.LogWarning("Login failed for user: {Username}", model.Username);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Admin requested user list.");

            var users = await _userManager.Users
                .Select(u => new { u.Id, u.UserName, u.Email, u.Role, u.FullName })
                .ToListAsync();

            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", id);
                return NotFound(new { message = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User deleted successfully: {UserId}", id);
                return Ok(new { message = "User deleted successfully." });
            }

            _logger.LogError("Failed to delete user: {UserId}", id);
            return BadRequest(new { message = "Failed to delete user." });
        }
    }
}
