using Microsoft.EntityFrameworkCore;
using UserProfileManagement.Models;

namespace UserProfileManagement.Data
{
    /// <summary>
    /// Application database context.
    /// Responsible for connecting the ASP.NET application
    /// with the SQL Server database using Entity Framework Core.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // Constructor
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Represents the UserProfiles table in the database.
        /// </summary>
        public DbSet<UserProfile> UserProfiles { get; set; }

        /// <summary>
        /// Configure database tables and constraints.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserProfile table
            modelBuilder.Entity<UserProfile>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.UserId);

                // Email must be unique
                entity.HasIndex(e => e.Email)
                      .IsUnique();

                // Maximum column lengths
                entity.Property(e => e.FullName)
                      .HasMaxLength(100);

                entity.Property(e => e.Email)
                      .HasMaxLength(150);

                entity.Property(e => e.PhoneNumber)
                      .HasMaxLength(10);

                entity.Property(e => e.Address)
                      .HasMaxLength(250);

                entity.Property(e => e.ProfilePicture)
                      .HasMaxLength(255);
            });
        }
    }
}