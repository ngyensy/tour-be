using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Models.Users;
using WebApi.Services;

[ApiController]
[Route("v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _redis;

    public AuthController(IUserService userService, IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _userService = userService;
        _configuration = configuration;
        _redis = redis;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest model)
    {
        var user = _userService.Authenticate(model.Username, model.Password);
        if (user == null)
            return Unauthorized(new { message = "Tên người dùng hoặc mật khẩu không hợp lệ" });

        var tokenString = GenerateJwtToken(user);
        SaveTokenToRedis(user.Id, tokenString);

        return Ok(new
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Role = user.Role,
            Token = tokenString,
            Avatar = user.Avatar,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            RewardPoints = user.RewardPoints,
            NumberOfToursTaken = user.NumberOfToursTaken,
        });
    }

    [Authorize]
    [HttpGet("secure-endpoint")]
    public IActionResult SecureEndpoint()
    {
        return Ok("Bạn đã có quyền truy cập vào endpoint bảo mật này.");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.Name);
        if (userId != null)
        {
            var db = _redis.GetDatabase();
            db.KeyDelete($"jwt:{userId}");
        }
        return Ok(new { message = "Đăng xuất thành công" });
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] string refreshToken)
    {
        var db = _redis.GetDatabase();
        var storedToken = db.StringGet($"refresh:{refreshToken}");

        if (string.IsNullOrEmpty(storedToken))
            return Unauthorized(new { message = "Refresh token không hợp lệ" });

        var userId = storedToken.ToString();
        var user = _userService.GetById(int.Parse(userId));
        if (user == null)
            return Unauthorized(new { message = "Người dùng không hợp lệ" });

        var tokenString = GenerateJwtToken(user);
        return Ok(new { Token = tokenString, RefreshToken = refreshToken });
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(2), // Cân nhắc điều chỉnh cho môi trường sản xuất
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private void SaveTokenToRedis(int userId, string tokenString)
    {
        var db = _redis.GetDatabase();
        db.StringSet($"jwt:{userId}", tokenString, TimeSpan.FromHours(2));
    }
}
