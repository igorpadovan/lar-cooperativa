using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using LarCooperativa.Api.Auth;
using LarCooperativa.Api.Data;
using LarCooperativa.Api.Data.Repositories;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Services;
using LarCooperativa.Api.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Configuração 'Jwt' ausente.");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        });

// Seguro por padrão: todo endpoint exige usuário autenticado, exceto os marcados com [AllowAnonymous]
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<ITelefoneRepository, TelefoneRepository>();
builder.Services.AddScoped<ITelefoneService, TelefoneService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Semeia o usuário administrador na primeira execução, com credenciais vindas
    // do ambiente (ADMIN_USUARIO/ADMIN_SENHA no .env) — sem padrão embutido no código
    if (!await db.Usuarios.AnyAsync())
    {
        var nomeUsuario = app.Configuration["Auth:AdminUsuario"]
            ?? throw new InvalidOperationException(
                "Configuração 'Auth:AdminUsuario' ausente: defina ADMIN_USUARIO no .env.");
        var senha = app.Configuration["Auth:AdminSenha"]
            ?? throw new InvalidOperationException(
                "Configuração 'Auth:AdminSenha' ausente: defina ADMIN_SENHA no .env.");

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var senhaHash = hasher.HashPassword(null!, senha); // o hasher padrão não usa o parâmetro user
        db.Usuarios.Add(new Usuario(nomeUsuario, senhaHash));
        await db.SaveChangesAsync();
        app.Logger.LogInformation("Usuário administrador {NomeUsuario} criado", nomeUsuario);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    // UI apenas para facilitar testes manuais dos endpoints em desenvolvimento
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "LarCooperativa API v1");
    });
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expõe a classe Program para o WebApplicationFactory dos testes de integração
public partial class Program;
