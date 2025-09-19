using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    public class Rent
    {
        [Key]
        public int RentID { get; set; }
        
        [ForeignKey("Booking")]
        public int BookingID { get; set; }
        
        [Required]
        public int OdometerStart { get; set; }
        
        public int? OdometerEnd { get; set; }
        
        [Required]
        public DateTime RentDate { get; set; }
        
        public DateTime? ActualReturnDate { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual Return? Return { get; set; }
    }
}