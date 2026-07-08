using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;

namespace LarCooperativa.Api.Services;

public interface IAuthService
{
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
