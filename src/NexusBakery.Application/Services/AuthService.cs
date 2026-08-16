using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexusBakery.Application.DTOs.Auth;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> GoogleLoginAsync(GoogleAuthRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            EmailVerified = false,
            Phone = request.Phone?.Trim(),
            PasswordHash = passwordHash,
            AuthProvider = AuthProviderType.Manual,
            Role = UserRole.User,
            Subscription = new UserSubscriptionInfo
            {
                Tier = SubscriptionTier.FreeLearner,
                IsActive = true
            }
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
        return await GenerateAuthResponseAsync(createdUser, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await GenerateAuthResponseAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleAuthRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required from Google authentication.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                FullName = request.FullName ?? "Google User",
                Email = request.Email.Trim().ToLowerInvariant(),
                EmailVerified = true,
                GoogleId = request.IdToken,
                AvatarUrl = request.AvatarUrl,
                AuthProvider = AuthProviderType.Google,
                Role = UserRole.User,
                Subscription = new UserSubscriptionInfo
                {
                    Tier = SubscriptionTier.FreeLearner,
                    IsActive = true
                }
            };
            user = await _userRepository.CreateAsync(user, cancellationToken);
        }
        else if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = request.IdToken;
            user.EmailVerified = true;
            if (!string.IsNullOrEmpty(request.AvatarUrl) && string.IsNullOrEmpty(user.AvatarUrl))
            {
                user.AvatarUrl = request.AvatarUrl;
            }
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return await GenerateAuthResponseAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.FindAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken && t.RevokedAt == null), cancellationToken);
        var user = users.FirstOrDefault();
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var tokenRecord = user.RefreshTokens.First(t => t.Token == refreshToken);
        if (tokenRecord.ExpiresAt < DateTime.UtcNow)
        {
            tokenRecord.RevokedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        // Revoke the old token and issue a fresh one
        tokenRecord.RevokedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return await GenerateAuthResponseAsync(user, ipAddress, cancellationToken);
    }

    public async Task<bool> RevokeTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            return await _userRepository.UpdateAsync(user, cancellationToken);
        }
        return false;
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : MapToProfileDto(user);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Phone)) user.Phone = request.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(request.AvatarUrl)) user.AvatarUrl = request.AvatarUrl.Trim();
        if (request.ShippingAddresses != null) user.ShippingAddresses = request.ShippingAddresses;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToProfileDto(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return false;

        var valid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!valid) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        return await _userRepository.UpdateAsync(user, cancellationToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string ipAddress, CancellationToken cancellationToken)
    {
        var jwtSecret = _configuration["JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? "NexusBakerySuperSecretKey2026_ArtisanTechBakeryPro!";

        var issuer = _configuration["JWT_ISSUER"] ?? "NexusBakeryApi";
        var audience = _configuration["JWT_AUDIENCE"] ?? "NexusBakeryClient";
        var expiryMinutes = int.TryParse(_configuration["JWT_EXPIRES_IN_MINUTES"], out var m) ? m : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("tier", user.Subscription?.Tier.ToString() ?? SubscriptionTier.FreeLearner.ToString()),
            new("isSubscribed", (user.Subscription?.IsActive == true).ToString().ToLowerInvariant())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate Refresh Token
        var refreshTokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(refreshTokenBytes);
        var refreshTokenString = Convert.ToBase64String(refreshTokenBytes);

        var refreshTokenRecord = new RefreshTokenRecord
        {
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedByIp = ipAddress,
            RevokedAt = null
        };

        await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshTokenRecord, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = expiresAt,
            User = MapToProfileDto(user)
        };
    }

    private static UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            Subscription = user.Subscription,
            ShippingAddresses = user.ShippingAddresses,
            CreatedAt = user.CreatedAt
        };
    }
}
