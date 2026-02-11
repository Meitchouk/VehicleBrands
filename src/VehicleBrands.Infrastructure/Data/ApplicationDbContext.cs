using Microsoft.EntityFrameworkCore;
using VehicleBrands.Domain.Entities;

namespace VehicleBrands.Infrastructure.Data;

/// <summary>
/// Application database context.
/// Configures the PostgreSQL connection and entity model.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for car brands.
    /// </summary>
    public DbSet<CarBrand> CarBrands => Set<CarBrand>();

    /// <summary>
    /// Configures the database model including the MarcasAutos table and data seed.
    /// Table is named "MarcasAutos" as required by the specification.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CarBrand>(entity =>
        {
            entity.ToTable("MarcasAutos");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CountryOfOrigin)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.FoundedYear)
                .IsRequired();

            entity.Property(e => e.Website)
                .HasMaxLength(200);

            entity.Property(e => e.IsLuxury)
                .HasDefaultValue(false);

            entity.Property(e => e.Headquarters)
                .HasMaxLength(150);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Unique index to prevent duplicate brand names
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });
    }
}
