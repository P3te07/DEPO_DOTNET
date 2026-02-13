using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Companie> Companii { get; set; }
    public DbSet<Depozit> Depozite { get; set; }
    public DbSet<Marfa> Marfuri { get; set; }
    public DbSet<Tranzactii> Tranzactii { get; set; }
    public DbSet<LogActivitate> LogActivitati { get; set; }
    public DbSet<Sarcina> Sarcini { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Companie 
        modelBuilder.Entity<Companie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanieId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Nume).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.CompanieId).IsUnique();
        });

        // Depozit 
        modelBuilder.Entity<Depozit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DepozitId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Nume).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.DepozitId).IsUnique();

            entity.HasOne(d => d.Companie)
                .WithMany(c => c.Depozite)
                .HasForeignKey(d => d.CompanieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Marfa 
        modelBuilder.Entity<Marfa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MarfaId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PretUnitar).HasPrecision(18, 2);

            entity.HasOne(m => m.Depozit)
                .WithMany(d => d.Marfuri)
                .HasForeignKey(m => m.DepozitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Tranzactie 
        modelBuilder.Entity<Tranzactii>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TranzactieId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ValoareTotala).HasPrecision(18, 2);

            entity.HasOne(t => t.DepozitSursa)
                .WithMany(d => d.TranzactiiSursa)
                .HasForeignKey(t => t.DepozitSursaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.DepozitDestinatie)
                .WithMany(d => d.TranzactiiDestinatie)
                .HasForeignKey(t => t.DepozitDestinatieId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}