using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.DTOs
{
    public class StreamlinedBookingRequestDTO
    {
        [Required]
        public int CarID { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }
    }
}