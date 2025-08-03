using EGMS.Models;
using Microsoft.EntityFrameworkCore;
using EGMS.DTOs;

namespace EGMS.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ElectricBill> ElectricBills { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(15);
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);

                // Create unique indexes
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.C_ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.F_name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.M_name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Mobile_number).IsRequired().HasMaxLength(15);
                entity.Property(e => e.NID_Number).IsRequired().HasMaxLength(17);
                entity.Property(e => e.Created_Date).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UserId).IsRequired();

                // Create unique indexes (scoped to user)
                entity.HasIndex(e => new { e.Mobile_number, e.UserId }).IsUnique();
                entity.HasIndex(e => new { e.NID_Number, e.UserId }).IsUnique();

                // Configure decimal properties
                entity.Property(e => e.Previous_Unit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Advance_money).HasColumnType("decimal(18,2)");

                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Customers)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ElectricBill entity
            modelBuilder.Entity<ElectricBill>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Previous_unit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Current_Unit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total_Unit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Electric_bill).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Previous_duos).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Rent_Bill).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Loan).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total_bill).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Clear_money).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Present_dues).HasColumnType("decimal(18,2)");

                // Configure foreign key relationship
                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.ElectricBills)
                      .HasForeignKey(e => e.Customer_ID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public DbSet<EGMS.DTOs.ElectricBillDTO> ElectricBillDTO { get; set; } = default!;
    }
}