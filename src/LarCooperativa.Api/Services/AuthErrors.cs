using LarCooperativa.Api.Common;

namespace LarCooperativa.Api.Services;

public static class AuthErrors
{
    // Mensagem única para usuário inexistente e senha errada: não revelar qual credencial falhou
    public static readonly Error CredenciaisInvalidas = new(
        ErrorType.Unauthorized, "credenciais_invalidas", "Usuário ou senha inválidos.");
}
