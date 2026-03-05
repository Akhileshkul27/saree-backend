using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using SareeGrace.Domain.Entities;
using SareeGrace.Infrastructure.Data;

namespace SareeGrace.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
            return ApiResponse<AuthResponseDto>.FailResponse("Email is already registered");

        var user = new User
        {
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Phone = dto.Phone.Trim()
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var tokens = await GenerateTokens(user);
        return ApiResponse<AuthResponseDto>.SuccessResponse(tokens, "Registration successful");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponse<AuthResponseDto>.FailResponse("Invalid email or password");

        if (!user.IsActive)
            return ApiResponse<AuthResponseDto>.FailResponse("Account is deactivated");

        var tokens = await GenerateTokens(user);
        return ApiResponse<AuthResponseDto>.SuccessResponse(tokens, "Login successful");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive)
            return ApiResponse<AuthResponseDto>.FailResponse("Invalid refresh token");

        // Revoke old token
        storedToken.RevokedAt = DateTime.UtcNow;
        var tokens = await GenerateTokens(storedToken.User);
        storedToken.ReplacedByToken = tokens.RefreshToken;
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.SuccessResponse(tokens, "Token refreshed");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(Guid userId, string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Token == refreshToken);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return ApiResponse<bool>.SuccessResponse(true, "Logged out successfully");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ApiResponse<UserDto>.FailResponse("User not found");

        return ApiResponse<UserDto>.SuccessResponse(MapToDto(user));
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ApiResponse<UserDto>.FailResponse("User not found");

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Phone = dto.Phone.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<UserDto>.SuccessResponse(MapToDto(user), "Profile updated");
    }

    private async Task<AuthResponseDto> GenerateTokens(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "SareeGraceSecretKey2026ForJWTAuth!@#$%^&*()";
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.GivenName, user.FirstName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "SareeGrace",
            audience: _config["Jwt:Audience"] ?? "SareeGraceApp",
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiry,
            User = MapToDto(user)
        };
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Phone = user.Phone,
        AvatarUrl = user.AvatarUrl,
        Role = user.Role
    };
}
