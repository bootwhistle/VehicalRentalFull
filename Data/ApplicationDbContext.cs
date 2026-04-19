using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleRentalFull.Models;

namespace VehicleRentalFull.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Billing> Billings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between Billing and Reservation
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Reservation)
                .WithOne(r => r.Billing)
                .HasForeignKey<Billing>(b => b.ReservationId);

            // Unique index on Billing.ReservationId
            modelBuilder.Entity<Billing>()
                .HasIndex(b => b.ReservationId)
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<Billing>()
                .Property(b => b.BaseCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Billing>()
                .Property(b => b.TaxAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Billing>()
                .Property(b => b.AdditionalCharges)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Billing>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.DailyRate)
                .HasColumnType("decimal(18,2)");
        }
    }
}
