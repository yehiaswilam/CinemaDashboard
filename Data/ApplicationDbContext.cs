using CinemaDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<MovieImage> MovieImages => Set<MovieImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Movie ----------
        modelBuilder.Entity<Movie>(e =>
        {
            e.Property(m => m.Price).HasPrecision(18, 2);
            e.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

            // Don't allow deleting a Category / Cinema that still has movies
            e.HasOne(m => m.Category)
             .WithMany(c => c.Movies)
             .HasForeignKey(m => m.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Cinema)
             .WithMany(c => c.Movies)
             .HasForeignKey(m => m.CinemaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(m => m.SubImages)
             .WithOne(i => i.Movie)
             .HasForeignKey(i => i.MovieId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- MovieActor (Many-to-Many) ----------
        modelBuilder.Entity<MovieActor>(e =>
        {
            e.HasKey(ma => new { ma.MovieId, ma.ActorId });

            e.HasOne(ma => ma.Movie)
             .WithMany(m => m.MovieActors)
             .HasForeignKey(ma => ma.MovieId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ma => ma.Actor)
             .WithMany(a => a.MovieActors)
             .HasForeignKey(ma => ma.ActorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Seed some categories ----------
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Action", Description = "Action & adventure movies" },
            new Category { Id = 2, Name = "Comedy", Description = "Comedy movies" },
            new Category { Id = 3, Name = "Drama", Description = "Drama movies" },
            new Category { Id = 4, Name = "Horror", Description = "Horror & thriller movies" },
            new Category { Id = 5, Name = "Sci-Fi", Description = "Science fiction movies" }
        );
    }
}
