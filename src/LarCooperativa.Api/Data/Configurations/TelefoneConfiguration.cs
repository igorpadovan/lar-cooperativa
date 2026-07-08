using LarCooperativa.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LarCooperativa.Api.Data.Configurations;

public sealed class TelefoneConfiguration : IEntityTypeConfiguration<Telefone>
{
    public void Configure(EntityTypeBuilder<Telefone> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Numero)
            .HasMaxLength(11)
            .IsRequired();

        // Uma pessoa não pode ter o mesmo número repetido
        builder.HasIndex(t => new { t.PessoaId, t.Numero })
            .IsUnique();

        builder.HasOne<Pessoa>()
            .WithMany()
            .HasForeignKey(t => t.PessoaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
