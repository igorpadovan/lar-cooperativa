namespace LarCooperativa.Api.Contracts;

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresInSeconds);
