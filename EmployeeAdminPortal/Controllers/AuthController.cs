// File: Controllers/AuthController.cs

using Microsoft.AspNetCore.Mvc;
using EmployeeAdminPortal.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _env = env;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { message = "Validation failed", errors });
                }

                var existingUser = await _userManager.FindByNameAsync(request.Username)
                               ?? await _userManager.FindByEmailAsync(request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists!" });
                }

                var user = new IdentityUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync("User"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    return Ok(new { message = "User registered successfully!" });
                }

                var errorMessages = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User registration failed: {Errors}", string.Join(';', errorMessages));
                return BadRequest(new { message = "Registration failed", errors = errorMessages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                if (_env.IsDevelopment())
                {
                    return StatusCode(500, new { message = "An error occurred during registration.", detail = ex.Message });
                }
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Username)
                          ?? await _userManager.FindByEmailAsync(request.Username);

                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, userRoles);

                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                if (_env.IsDevelopment())
                {
                    return StatusCode(500, new { message = "An error occurred during login.", detail = ex.Message });
                }
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}