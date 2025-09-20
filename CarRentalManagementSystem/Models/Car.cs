using System.ComponentModel.DataAnnotations;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Models
{
    public class Car
    {
        [Key]
        public int CarID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CarName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string CarModel { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string CarBrand { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ImageUrl { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal RentPerDay { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PerKmRate { get; set; }
        
        [Required]
        [Range(1, 1000)]
        public int AllowedKmPerDay { get; set; } = 100;
        
        [Required]
        [StringLength(50)]
        public string CarType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string FuelType { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 20)]
        public int SeatingCapacity { get; set; }
        
        [Required]
        [Range(1, 100)]
        public double Mileage { get; set; }
        
        [Required]
        [StringLength(20)]
        public string NumberPlate { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Status { get; set; } = "Available";
        
        [StringLength(50)]
        public string? LastOdometerReading { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}