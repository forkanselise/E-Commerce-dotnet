using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Domain.Entities;

public class User : BaseEntity
{
    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("emailVerified")]
    public bool EmailVerified { get; set; } = false;

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("phoneVerified")]
    public bool PhoneVerified { get; set; } = false;

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("authProvider")]
    public AuthProviderType AuthProvider { get; set; } = AuthProviderType.Manual;

    [BsonElement("googleId")]
    public string? GoogleId { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("role")]
    public UserRole Role { get; set; } = UserRole.User;

    [BsonElement("subscription")]
    public UserSubscriptionInfo? Subscription { get; set; }

    [BsonElement("shippingAddresses")]
    public List<ShippingAddress> ShippingAddresses { get; set; } = new();

    [BsonElement("refreshTokens")]
    public List<RefreshTokenRecord> RefreshTokens { get; set; } = new();

    [BsonElement("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
}

public class UserSubscriptionInfo
{
    [BsonElement("tier")]
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.FreeLearner;

    [BsonElement("startedAt")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("autoRenew")]
    public bool AutoRenew { get; set; } = false;

    [BsonElement("paymentMethod")]
    public PaymentMethodType? PaymentMethod { get; set; }
}

public class ShippingAddress
{
    [BsonElement("label")]
    public string Label { get; set; } = "Home";

    [BsonElement("fullAddress")]
    public string FullAddress { get; set; } = string.Empty;

    [BsonElement("city")]
    public string City { get; set; } = "Dhaka";

    [BsonElement("postalCode")]
    public string? PostalCode { get; set; }

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("isDefault")]
    public bool IsDefault { get; set; } = false;
}

public class RefreshTokenRecord
{
    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("createdByIp")]
    public string CreatedByIp { get; set; } = string.Empty;

    [BsonElement("revokedAt")]
    public DateTime? RevokedAt { get; set; }
}
