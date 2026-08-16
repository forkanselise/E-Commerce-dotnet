namespace NexusBakery.Domain.Enums;

public enum UserRole
{
    User,
    Admin,
    SystemAdmin
}

public enum AuthProviderType
{
    Manual,
    Google,
    Phone
}

public enum ProductCategory
{
    Bakery,
    BakingTools,
    Electronics
}

public enum PaymentMethodType
{
    Bkash,
    Nagad,
    Card,
    Cod
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded,
    CodPending
}

public enum OrderType
{
    ECommerce,
    TutorialSubscription,
    TutorialPurchase
}

public enum OrderStatus
{
    Placed,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum SubscriptionTier
{
    FreeLearner,
    MasterclassPro,
    VipBakerPass
}

public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Master
}
