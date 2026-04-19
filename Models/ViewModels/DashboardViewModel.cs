namespace VehicleRentalFull.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int ActiveReservations { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public List<RecentReservationItem> RecentReservations { get; set; } = new List<RecentReservationItem>();
    }

    public class RecentReservationItem
    {
        public int ReservationId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
