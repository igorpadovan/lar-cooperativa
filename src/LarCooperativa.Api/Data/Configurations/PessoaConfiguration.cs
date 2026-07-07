using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LarCooperativa.Api.Data.Configurations;

public sealed class PessoaConfiguration : IEntityTypeConfiguration<Pessoa>
{
    public void Configure(EntityTypeBuilder<Pessoa> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Cpf)
            .HasConversion(cpf => cpf.Valor, valor => Cpf.FromTrusted(valor))
            .HasMaxLength(11)
            .IsFixedLength()
            .IsRequired();

        builder.HasIndex(p => p.Cpf)
            .IsUnique();

        builder.Property(p => p.DataNascimento)
            .IsRequired();

        builder.Property(p => p.EstaAtivo)
            .IsRequired();
    }
}
