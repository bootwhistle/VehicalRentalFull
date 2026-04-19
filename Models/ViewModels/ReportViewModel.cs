using System.ComponentModel.DataAnnotations;

namespace VehicleRentalFull.Models.ViewModels
{
    public class ReportViewModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Vehicle Category")]
        public string? Category { get; set; }

        public List<ReportLineItem>? Results { get; set; }

        public decimal TotalRevenue { get; set; }
        public string MostRentedVehicle { get; set; } = string.Empty;
        public string TopCustomer { get; set; } = string.Empty;
    }

    public class ReportLineItem
    {
        public int ReservationId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public int RentalDays { get; set; }
        public decimal TotalBilled { get; set; }
    }
}
