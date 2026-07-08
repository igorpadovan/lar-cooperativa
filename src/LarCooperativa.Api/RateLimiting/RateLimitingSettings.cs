namespace LarCooperativa.Api.RateLimiting;

public static class RateLimitPolicies
{
    /// <summary>Fixed window por IP, mais restrito — mitiga força bruta no login.</summary>
    public const string Login = "login";
}

public sealed class ApiRateLimitSettings
{
    public int PermitLimit { get; init; } = 100;
    public int WindowSeconds { get; init; } = 60;
    public int SegmentsPerWindow { get; init; } = 6;
}

public sealed class LoginRateLimitSettings
{
    public int PermitLimit { get; init; } = 10;
    public int WindowSeconds { get; init; } = 60;
}
