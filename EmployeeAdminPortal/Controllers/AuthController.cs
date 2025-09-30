// File: Controllers/AuthController.cs

using EmployeeAdminPortal.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // POST: /api/auth/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Email // Use the correct DTO property for Email
            };

            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if (identityResult.Succeeded)
            {
                // Assign "User" role to every registered user
                await _userManager.AddToRoleAsync(identityUser, "User");

                return Ok("User registered successfully! You can now log in.");
            }

            return BadRequest(identityResult.Errors);
        }

        // POST: /api/auth/login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            // ⭐️ FIX: Check if the input is an email, and try to find the user by email first,
            // otherwise, fall back to finding by username.
            IdentityUser? user;
            if (loginRequestDto.Username.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginRequestDto.Username);
            }
            else
            {
                user = await _userManager.FindByNameAsync(loginRequestDto.Username);
            }

            // If user is null after checking both, return Unauthorized
            if (user == null)
            {
                return Unauthorized("Login failed. Invalid credentials.");
            }

            // Check the password
            if (await _userManager.CheckPasswordAsync(user, loginRequestDto.Password))
            {
                var token = await GenerateJwtToken(user);

                return Ok(new { Token = token });
            }

            return Unauthorized("Login failed. Invalid credentials.");
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found.");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found.");
            var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found."); // ⭐️ ADDED: Audience check

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles to claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience, // ⭐️ ADDED: Set the Audience
                Expires = DateTime.Now.AddDays(7), // Token expiration time
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}