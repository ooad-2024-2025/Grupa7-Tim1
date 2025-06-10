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
        public DbSet<FixniTermin> FixniTermini { get; set; }  // DODANO
        public DbSet<Izostanak> Izostanak { get; set; }
        public DbSet<Ocjena> Ocjena { get; set; }
        public DbSet<PredmetRazred> PredmetRazred { get; set; }
        public DbSet<Poruka> Poruka { get; set; }

        public DbSet<EvidencijaCasa> EvidencijaCasa { get; set; }

        public DbSet<Aktivnost> Aktivnost { get; set; }
        public DbSet<ObavjestenjeLog> ObavjestenjeLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Razred>().ToTable("Razred");
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<UcenikPredmet>().ToTable("RazredPredmet");
            modelBuilder.Entity<Cas>().ToTable("Cas");
            modelBuilder.Entity<Izostanak>().ToTable("Izostanak");
            modelBuilder.Entity<Ocjena>().ToTable("Ocjena");

            modelBuilder.Entity<EvidencijaCasa>()
                .HasOne(e => e.Cas)
                .WithMany()
                .HasForeignKey(e => e.CasId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EvidencijaCasa>()
                .HasOne(e => e.Nastavnik)
                .WithMany()
                .HasForeignKey(e => e.NastavnikId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // PORUKA
            modelBuilder.Entity<Poruka>()
                .HasOne(p => p.Posiljalac)
                .WithMany()
                .HasForeignKey(p => p.PosiljalacId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Poruka>()
                .HasOne(p => p.Primalac)
                .WithMany()
                .HasForeignKey(p => p.PrimalacId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aktivnost>()
                .HasOne(a => a.Nastavnik)
                .WithMany()
                .HasForeignKey(a => a.NastavnikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aktivnost>()
                .HasOne(a => a.Razred)
                .WithMany()
                .HasForeignKey(a => a.RazredId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Aktivnost>()
                .HasOne(a => a.Predmet)
                .WithMany()
                .HasForeignKey(a => a.PredmetId)
                .OnDelete(DeleteBehavior.SetNull);

            // OBAVJEŠTENJE LOG
            modelBuilder.Entity<ObavjestenjeLog>()
                .HasOne(o => o.Aktivnost)
                .WithMany()
                .HasForeignKey(o => o.AktivnostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ObavjestenjeLog>()
                .HasOne(o => o.Korisnik)
                .WithMany()
                .HasForeignKey(o => o.KorisnikId)
                .OnDelete(DeleteBehavior.Restrict);

            // FIKSNI TERMINI SEED DATA - DODANO
            modelBuilder.Entity<FixniTermin>().HasData(
                FixniTermin.GetStandardniTermini().ToArray()
            );
        }
    }
}