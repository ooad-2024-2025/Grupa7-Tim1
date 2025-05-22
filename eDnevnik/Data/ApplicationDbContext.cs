using eDnevnik.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eDnevnik.Data
{
    public class ApplicationDbContext : IdentityDbContext<Korisnik>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Razred> Razred { get; set; }
        public DbSet<Predmet> Predmet { get; set; }
        public DbSet<UcenikPredmet> RazredPredmet { get; set; }
        public DbSet<Cas> Cas { get; set; }
        public DbSet<Izostanak> Izostanak { get; set; }
        public DbSet<Ocjena> Ocjena { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Razred>().ToTable("Razred");
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<UcenikPredmet>().ToTable("RazredPredmet");
            modelBuilder.Entity<Cas>().ToTable("Cas");
            modelBuilder.Entity<Izostanak>().ToTable("Izostanak");
            modelBuilder.Entity<Ocjena>().ToTable("Ocjena");

            modelBuilder.Entity<Korisnik>(b =>
            {
                b.Property(u => u.Ime);
                b.Property(u => u.Prezime);
                b.Property(u => u.Vladanje);
                b.Property(u => u.RazredId);
                b.Property(u => u.RoditeljId);
            });

            base.OnModelCreating(modelBuilder);

            // CAS
            modelBuilder.Entity<Cas>()
                .HasOne(c => c.Razred)
                .WithMany(r => r.Casovi)
                .HasForeignKey(c => c.RazredId)
                .OnDelete(DeleteBehavior.Restrict); // Samo ovde Cascade

            modelBuilder.Entity<Cas>()
                .HasOne(c => c.Predmet)
                .WithMany(p => p.Casovi)
                .HasForeignKey(c => c.PredmetId)
                .OnDelete(DeleteBehavior.Restrict); // Ovde Restrict

            modelBuilder.Entity<Cas>()
                .HasOne(c => c.Nastavnik)
                .WithMany()
                .HasForeignKey(c => c.NastavnikId)
                .OnDelete(DeleteBehavior.Restrict);

            // IZOSTANAK
            modelBuilder.Entity<Izostanak>()
                .HasOne(i => i.Ucenik)
                .WithMany()
                .HasForeignKey(i => i.UcenikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Izostanak>()
                .HasOne(i => i.Cas)
                .WithMany()
                .HasForeignKey(i => i.CasId)
                .OnDelete(DeleteBehavior.Restrict);

            // PREDMET
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Nastavnik)
                .WithMany()
                .HasForeignKey(p => p.NastavnikId)
                .OnDelete(DeleteBehavior.Restrict);

            // RAZRED
            modelBuilder.Entity<Razred>()
                .HasOne(r => r.Nastavnik)
                .WithMany()
                .HasForeignKey(r => r.NastavnikId)
                .OnDelete(DeleteBehavior.Restrict);

            // OCJENA
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Ucenik)
                .WithMany()
                .HasForeignKey(o => o.UcenikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Predmet)
                .WithMany(p => p.Ocjene)
                .HasForeignKey(o => o.PredmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // KORISNIK
            modelBuilder.Entity<Korisnik>()
                .HasOne(k => k.Razred)
                .WithMany()
                .HasForeignKey(k => k.RazredId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Korisnik>()
                .HasOne(k => k.Roditelj)
                .WithMany()
                .HasForeignKey(k => k.RoditeljId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}