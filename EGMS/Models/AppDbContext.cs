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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

                // Create unique indexes
                entity.HasIndex(e => e.Mobile_number).IsUnique();
                entity.HasIndex(e => e.NID_Number).IsUnique();

                // Configure decimal properties
                entity.Property(e => e.Previous_Unit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Advance_money).HasColumnType("decimal(18,2)");
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