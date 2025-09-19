using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CarRentalManagementSystem.DTOs
{
    public class CarBookingRequestDTO
    {
        [Required]
        public int CarID { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;
        
        public IFormFile? LicenseFrontImage { get; set; }
        
        public IFormFile? LicenseBackImage { get; set; }
    }
}