using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ProgrammingPOE.Models;

namespace ProgrammingPOE.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.HourlyRate)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Claim entity
            builder.Entity<Claim>(entity =>
            {
                entity.HasKey(c => c.ClaimId);

                entity.Property(c => c.TotalAmount)
                    .HasColumnType("decimal(18,2)");
                entity.Property(c => c.HourlyRate)
                    .HasColumnType("decimal(18,2)");
                entity.Property(c => c.HoursWorked)
                    .HasColumnType("decimal(18,2)");

                // Relationship with ApplicationUser (Lecturer)
                entity.HasOne(c => c.Lecturer)
                    .WithMany(u => u.Claims)
                    .HasForeignKey(c => c.LecturerUserId)
                    .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

                // Configure the collection of documents
                entity.HasMany(c => c.Documents)
                    .WithOne(d => d.Claim)
                    .HasForeignKey(d => d.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SupportingDocument entity
            builder.Entity<SupportingDocument>(entity =>
            {
                entity.HasKey(sd => sd.DocumentId);

                // Single relationship with Claim
                entity.HasOne(sd => sd.Claim)
                    .WithMany(c => c.Documents)
                    .HasForeignKey(sd => sd.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}