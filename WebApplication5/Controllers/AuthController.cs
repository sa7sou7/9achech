using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication5.Dto;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid registration data");
                    return BadRequest(ModelState);
                }

                var user = new IdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Failed to register user {model.Username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                if (!string.IsNullOrEmpty(model.Role) && new[] { "Admin", "Manager", "Commercial" }.Contains(model.Role))
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                else
                {
                    _logger.LogWarning($"Invalid role {model.Role} provided for user {model.Username}");
                    return BadRequest(new { error = "Invalid role. Use Admin, Manager, or Commercial." });
                }

                _logger.LogInformation($"User {model.Username} registered successfully with role {model.Role}");
                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user {model.Username}");
                return StatusCode(500, new { error = "Failed to register user: " + ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid login data");
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    _logger.LogWarning($"Invalid login attempt for user {model.Username}");
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles);

                _logger.LogInformation($"User {model.Username} logged in successfully");
                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging in user {model.Username}");
                return StatusCode(500, new { error = "Failed to login: " + ex.Message });
            }
        }

        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}