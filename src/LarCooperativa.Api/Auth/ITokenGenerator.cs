using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Auth;

public interface ITokenGenerator
{
    TokenResponse Generate(Usuario usuario);
}
