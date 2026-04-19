using System.ComponentModel.DataAnnotations;

namespace VehicleRentalFull.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        public string Category { get; set; } = "Sedan";

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Daily rate must be greater than 0")]
        [Display(Name = "Daily Rate ($)")]
        public decimal DailyRate { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
