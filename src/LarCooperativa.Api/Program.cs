using LarCooperativa.Api.Data;
using LarCooperativa.Api.Data.Repositories;
using LarCooperativa.Api.Domain;
using LarCooperativa.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

app.Run();

// Expõe a classe Program para o WebApplicationFactory dos testes de integração
public partial class Program;
