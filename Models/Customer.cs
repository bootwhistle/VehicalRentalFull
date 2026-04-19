using System.ComponentModel.DataAnnotations;

namespace VehicleRentalFull.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Driver's License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Display(Name = "Member Since")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
