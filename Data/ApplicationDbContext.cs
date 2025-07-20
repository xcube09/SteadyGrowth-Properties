using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Data
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Use 'new' to hide inherited member for Users
        public new DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
		public DbSet<Referral> Referrals { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<VettingLog> VettingLogs { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<AcademyPackage> AcademyPackages { get; set; }
        public DbSet<UpgradeRequest> UpgradeRequests { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<CourseProgress> CourseProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Property configurations
            builder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Properties)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Referral configurations
            builder.Entity<Referral>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CommissionEarned).HasPrecision(18, 2);
                
                entity.HasOne(e => e.Referrer)
                      .WithMany(u => u.ReferralsMade)
                      .HasForeignKey(e => e.ReferrerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReferredUser)
                      .WithMany(u => u.ReferralsReceived)
                      .HasForeignKey(e => e.ReferredUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ReferrerId);
                entity.HasIndex(e => e.ReferredUserId);
            });

            // User configurations
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.ReferralCode)
                      .IsUnique()
                      .HasFilter("[ReferralCode] IS NOT NULL");

                entity.HasOne(u => u.AcademyPackage)
                      .WithMany(ap => ap.Users)
                      .HasForeignKey(u => u.AcademyPackageId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // PropertyImage configurations
            builder.Entity<PropertyImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Caption).HasMaxLength(500);
                entity.Property(e => e.ImageType).HasMaxLength(100);
                entity.HasOne(e => e.Property)
                      .WithMany(p => p.PropertyImages)
                      .HasForeignKey(e => e.PropertyId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // Course configurations
            builder.Entity<Course>(entity =>
            {
                entity.HasOne(c => c.AcademyPackage)
                      .WithMany(ap => ap.Courses)
                      .HasForeignKey(c => c.AcademyPackageId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // UpgradeRequest configurations
            builder.Entity<UpgradeRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(ur => ur.User)
                      .WithMany()
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ur => ur.RequestedPackage)
                      .WithMany()
                      .HasForeignKey(ur => ur.RequestedPackageId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Wallet configurations
            builder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Wallet)
                      .HasForeignKey<Wallet>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // WalletTransaction configurations
            builder.Entity<WalletTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.BalanceBefore).HasPrecision(18, 2);
                entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
                entity.HasOne(e => e.Wallet)
                      .WithMany(w => w.Transactions)
                      .HasForeignKey(e => e.WalletId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AdminUser)
                      .WithMany()
                      .HasForeignKey(e => e.AdminUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.WalletId);
                entity.HasIndex(e => e.TransactionType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Add further entity configurations here as needed

            builder.Entity<CourseProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(cp => cp.User)
                      .WithMany()
                      .HasForeignKey(cp => cp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.Course)
                      .WithMany()
                      .HasForeignKey(cp => cp.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            });
        }
    }
}
