using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LarCooperativa.IntegrationTests;

/// <summary>
/// Fábrica apontada para um banco exclusivo do teste, para exercitar a inicialização
/// (migração + seed do admin) a partir de um banco que ainda não existe.
/// </summary>
file sealed class BancoNovoApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        // Credenciais fixas: o banco novo sempre passa pelo seed, que exige esta configuração
        builder.UseSetting("Auth:AdminUsuario", "admin");
        builder.UseSetting("Auth:AdminSenha", "admin123");
    }
}

/// <summary>
/// Reproduz o cenário do CI (e de réplicas em produção): várias instâncias da API
/// sobem ao mesmo tempo contra um banco que ainda não existe — migração e seed
/// não podem conflitar entre si.
/// </summary>
public sealed class InicializacaoConcorrenteTests : IAsyncLifetime
{
    private const int QuantidadeDeInstancias = 4;

    private readonly string _connectionString;
    private readonly string _bancoDeDados;

    public InicializacaoConcorrenteTests()
    {
        // Mesma resolução de configuração da API: env ConnectionStrings__Default
        // (compose) sobrepõe o appsettings.Development.json (execução no host)
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionStringBase = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' ausente: defina ConnectionStrings__Default.");

        var construtor = new NpgsqlConnectionStringBuilder(connectionStringBase);
        _bancoDeDados = $"{construtor.Database}_inicializacao";
        construtor.Database = _bancoDeDados;
        _connectionString = construtor.ConnectionString;
    }

    public Task InitializeAsync() => RemoverBancoAsync();

    public Task DisposeAsync() => RemoverBancoAsync();

    [Fact]
    public async Task Inicializacao_ComInstanciasConcorrentesEmBancoNovo_MigraESemeiaSemConflito()
    {
        var factories = Enumerable.Range(0, QuantidadeDeInstancias)
            .Select(_ => new BancoNovoApiFactory(_connectionString))
            .ToArray();

        try
        {
            await Task.WhenAll(factories.Select(factory => Task.Run(factory.CreateClient)));

            Assert.Equal(1L, await ContarUsuariosAsync());
        }
        finally
        {
            foreach (var factory in factories)
            {
                await factory.DisposeAsync();
            }
        }
    }

    private async Task<long> ContarUsuariosAsync()
    {
        await using var conexao = new NpgsqlConnection(_connectionString);
        await conexao.OpenAsync();
        await using var comando = new NpgsqlCommand("""SELECT COUNT(*) FROM "Usuarios";""", conexao);
        return (long)(await comando.ExecuteScalarAsync())!;
    }

    private async Task RemoverBancoAsync()
    {
        var construtor = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = "postgres",
            Pooling = false,
        };
        await using var conexao = new NpgsqlConnection(construtor.ConnectionString);
        await conexao.OpenAsync();
        await using var comando = new NpgsqlCommand(
            $"""DROP DATABASE IF EXISTS "{_bancoDeDados}" WITH (FORCE);""", conexao);
        await comando.ExecuteNonQueryAsync();
    }
}
