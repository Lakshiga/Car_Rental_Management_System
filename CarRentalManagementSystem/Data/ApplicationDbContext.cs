using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Rent> Rents { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserID);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.UserID);

            // Booking relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerID);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Car)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CarID);

            // Rent relationships
            modelBuilder.Entity<Rent>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Rent)
                .HasForeignKey<Rent>(r => r.BookingID);

            // Return relationships
            modelBuilder.Entity<Return>()
                .HasOne(r => r.Rent)
                .WithOne(rent => rent.Return)
                .HasForeignKey<Return>(r => r.RentID);

            // Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingID);

            // Decimal precision
            modelBuilder.Entity<Car>()
                .Property(c => c.RentPerDay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Car>()
                .Property(c => c.PerKmRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Return>()
                .Property(r => r.ExtraCharge)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Return>()
                .Property(r => r.TotalDue)
                .HasPrecision(18, 2);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.Now
                }
            );

            // Note: Cars should only be added by Admin through the dashboard
            // No hardcoded cars in seed data
        }
    }
}