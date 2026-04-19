using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalFull.Models
{
    public class Billing
    {
        [Key]
        public int BillingId { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }

        [Display(Name = "Base Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseCost { get; set; }

        [Display(Name = "Tax (10%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Display(Name = "Additional Charges")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalCharges { get; set; } = 0;

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Paid")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "Paid At")]
        public DateTime? PaidAt { get; set; }
    }
}
