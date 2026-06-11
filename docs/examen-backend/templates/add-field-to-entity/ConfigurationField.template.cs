// PLANTILLA DE ESTUDIO — NO COMPILAR
// Modificar: Infrastructure/Persistence/Configurations/{Entidad}Configuration.cs
// Referencia: Infrastructure/Persistence/Configurations/VehicleConfiguration.cs — Plate

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        // ... configuración existente ...

        // NUEVO CAMPO — ejemplo obligatorio con longitud máxima
        // builder.Property(x => x.NewField)
        //     .IsRequired()
        //     .HasMaxLength(50);

        // Índice único si el negocio lo exige (como Plate):
        // builder.HasIndex(x => x.NewField)
        //     .IsUnique();

        // Campo opcional:
        // builder.Property(x => x.OptionalField)
        //     .HasMaxLength(100);
    }
}
