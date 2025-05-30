using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Collections.Concurrent;
using EduSync_Assessment.Models; // Adjust to your actual namespace
using EduSync_Assessment.Data;   // Assuming your DbContext is here

namespace EduSync_Assessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<string, (string email, DateTime expiry)> _resetTokens = new();

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _context.UserTables.FirstOrDefault(u => u.Email == loginDto.Email);

            if (user == null || user.PasswordHash != loginDto.Password) // Replace with hashed check in production
                return Unauthorized(new { message = "Invalid email or password." });

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                user = new
                {
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var user = _context.UserTables.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Ok(new { message = "If an account exists with this email, a password reset link will be sent." });

            var token = Guid.NewGuid().ToString();
            _resetTokens.TryAdd(token, (request.Email, DateTime.UtcNow.AddHours(1)));

            // In a real application, send this token via email
            // For development, we'll return it in the response
            return Ok(new { token, message = "Password reset token generated successfully." });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (!_resetTokens.TryGetValue(request.Token, out var tokenInfo))
                return BadRequest(new { message = "Invalid or expired token." });

            if (DateTime.UtcNow > tokenInfo.expiry)
            {
                _resetTokens.TryRemove(request.Token, out _);
                return BadRequest(new { message = "Token has expired." });
            }

            var user = _context.UserTables.FirstOrDefault(u => u.Email == tokenInfo.email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            user.PasswordHash = request.NewPassword;
            _context.SaveChanges();

            _resetTokens.TryRemove(request.Token, out _);

            return Ok(new { message = "Password has been reset successfully." });
        }

        private string GenerateJwtToken(UserTable user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "Student"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())

            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
