using Microsoft.EntityFrameworkCore;

namespace Cards.Api.Data;

public class CardsDbContext(DbContextOptions<CardsDbContext> options) : DbContext(options)
{
    public DbSet<CardEntity> Cards => Set<CardEntity>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<CardEntity>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Type).HasMaxLength(50);
            e.HasIndex(c => c.Name).IsUnique();
            
            e.HasData(
                new CardEntity { Id = 1, Name = "Blue-Eyes White Dragon", Type = "Dragon", Attack = 3000 },
                new CardEntity { Id = 2, Name = "Dark Magician", Type = "Spellcaster", Attack = 2500 },
                new CardEntity { Id = 3, Name = "Kuriboh", Type = "Fiend", Attack = 300 });
        });
    }
}