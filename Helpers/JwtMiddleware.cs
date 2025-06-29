using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Models;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;
    private readonly IConnectionMultiplexer _redis;

    public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings, IConnectionMultiplexer redis)
    {
        _next = next;
        _jwtSettings = jwtSettings.Value;
        _redis = redis;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            await AttachUserToContext(context, token);

        await _next(context);
    }

    private async Task AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userId = principal.FindFirst(ClaimTypes.Name)?.Value;

            // Kiểm tra token có hợp lệ không (tức là có tồn tại trong Redis)
            var db = _redis.GetDatabase();
            var storedToken = await db.StringGetAsync($"jwt:{userId}");

            if (storedToken == token)
            {
                context.Items["User"] = userId;
            }
        }
        catch
        {
            // do nothing if the token is invalid
        }
    }
}
