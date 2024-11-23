using System;
using System.ComponentModel.DataAnnotations;

namespace CozyPlaceSG.Models
{
    // Enum to represent the status of a booking
    public enum BookingStatus
    {
        Confirmed,
        Pending,
        Cancelled
    }


    public class Booking
    {

        [Key]
        public int BookingId { get; set; }


        [Required]
        [StringLength(100)]
        public string? FacilityName { get; set; }


        [Required]
        [StringLength(255)]
        public string? FacilityDescription { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDateFrom { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDateTo { get; set; }


        [Required]
        [StringLength(8)]
        public String? BookingTimeFrom { get; set; }

        [Required]
        [StringLength(8)]
        public String? BookingTimeTo { get; set; }


        [Required]
        [StringLength(50)]
        public string? BookedBy { get; set; }

        // Booking status, required and linked to the BookingStatus enum
        [Required]
        public BookingStatus BookingStatus { get; set; }
    }
}
