using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LarCooperativa.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();

    public DbSet<Telefone> Telefones => Set<Telefone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
