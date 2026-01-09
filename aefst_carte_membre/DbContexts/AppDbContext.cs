using aefst_carte_membre.Identity;
using aefst_carte_membre.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace aefst_carte_membre.DbContexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Membre> membres { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Membre>(entity =>
            {
                entity.ToTable("membres");
                entity.HasKey(e => e.Id);



                //Id
                //entity.Property(e => e.Id)
                //      .HasColumnName("id")
                //      .HasDefaultValueSql("uuid_generate_v4()");

                // Matricule (généré par trigger)
                entity.Property(e => e.Matricule)
                      .HasColumnName("matricule")
                      .ValueGeneratedOnAdd();

                // Nom et prénom
                entity.Property(e => e.Nom)
                      .HasColumnName("nom")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Prenom)
                      .HasColumnName("prenom")
                      .HasMaxLength(100)
                      .IsRequired();

                // Date et lieu de naissance
                entity.Property(e => e.DateNaissance)
                      .HasColumnName("date_naissance")
                      .HasColumnType("date")
                      .IsRequired();

                entity.Property(e => e.LieuNaissance)
                      .HasColumnName("lieu_naissance")
                      .HasMaxLength(100)
                      .IsRequired();

                // Option, cycle, niveau
                entity.Property(e => e.Option)
                      .HasColumnName("option")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Cycle)
                      .HasColumnName("cycle")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Niveau)
                      .HasColumnName("niveau")
                      .HasMaxLength(50)
                      .IsRequired();

                // Téléphone
                entity.Property(e => e.Telephone)
                      .HasColumnName("telephone")
                      .HasMaxLength(20)
                      .IsRequired();

                // Photo
                entity.Property(e => e.PhotoUrl)
                      .HasColumnName("photo_url")
                      .IsRequired();

                // Dates
                entity.Property(e => e.DateCreation)
                      .HasColumnName("date_creation")
                      .HasColumnType("timestamp")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                      //.IsRequired();

                entity.Property(e => e.DateExpiration)
                      .HasColumnName("date_expiration")
                      .HasColumnType("date")
                      .IsRequired();

                // Statut
                entity.Property(e => e.Statut)
                      .HasColumnName("statut")
                      .HasMaxLength(20)
                      .HasDefaultValue("ACTIF")
                      .IsRequired();

                entity.Property(e => e.UserId)
      .HasColumnName("user_id");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);


                // Index
                entity.HasIndex(e => e.Nom).HasDatabaseName("idx_membres_nom");
                entity.HasIndex(e => e.Matricule).HasDatabaseName("idx_membres_matricule");
                entity.HasIndex(e => e.Statut).HasDatabaseName("idx_membres_statut");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens"); // lowercase recommandé PostgreSQL

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Token)
                      .IsRequired();

                entity.Property(e => e.ExpiresAt)
                      .IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<string>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

          
            
            
            
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
