using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    public class Return
    {
        [Key]
        public int ReturnID { get; set; }
        
        [ForeignKey("Rent")]
        public int RentID { get; set; }
        
        [Required]
        public DateTime ReturnDate { get; set; }
        
        [Required]
        public int OdometerEnd { get; set; }
        
        [Required]
        public int ExtraKM { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ExtraCharge { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalDue { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Rent Rent { get; set; } = null!;
    }
}