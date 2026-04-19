using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalFull.Data;
using VehicleRentalFull.Models.ViewModels;

namespace VehicleRentalFull.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var totalVehicles = await _context.Vehicles.CountAsync();
            var availableVehicles = await _context.Vehicles.CountAsync(v => v.IsAvailable);
            var activeReservations = await _context.Reservations
                .CountAsync(r => r.Status != "Cancelled");
            var newCustomersThisMonth = await _context.Customers
                .CountAsync(c => c.CreatedAt >= startOfMonth);

            var recentReservations = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentReservationItem
                {
                    ReservationId = r.ReservationId,
                    CustomerName = r.Customer != null ? r.Customer.FullName : "Unknown",
                    VehicleName = r.Vehicle != null ? $"{r.Vehicle.Year} {r.Vehicle.Make} {r.Vehicle.Model}" : "Unknown",
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.Status
                })
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalVehicles = totalVehicles,
                AvailableVehicles = availableVehicles,
                ActiveReservations = activeReservations,
                NewCustomersThisMonth = newCustomersThisMonth,
                RecentReservations = recentReservations
            };

            return View(vm);
        }
    }
}
