// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Infrastructure/Persistence/Configurations/NewEntityConfiguration.cs
// Referencia: Infrastructure/Persistence/Configurations/GenderConfiguration.cs
//             Infrastructure/Persistence/Configurations/VehicleConfiguration.cs
//
// Se aplica automáticamente vía modelBuilder.ApplyConfigurationsFromAssembly en AppDbContext.

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        builder.ToTable("NewEntities");  // Nombre de tabla en MySQL

        builder.HasKey(x => x.NewEntityId);

        builder.Property(x => x.NewEntityId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(true);

        // Índice único si el negocio lo exige:
        builder.HasIndex(x => x.Name)
            .IsUnique();

        // Relación ejemplo (ajustar nombres):
        // builder.HasOne(x => x.Other)
        //     .WithMany(x => x.NewEntities)
        //     .HasForeignKey(x => x.OtherEntityId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
