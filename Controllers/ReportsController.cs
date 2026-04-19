using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalFull.Data;
using VehicleRentalFull.Models.ViewModels;

namespace VehicleRentalFull.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ReportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ReportViewModel vm)
        {
            var query = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.Billing)
                .Where(r => r.Status != "Cancelled")
                .AsQueryable();

            if (vm.StartDate.HasValue)
            {
                query = query.Where(r => r.StartDate >= vm.StartDate.Value);
            }

            if (vm.EndDate.HasValue)
            {
                query = query.Where(r => r.EndDate <= vm.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(vm.Category) && vm.Category != "All")
            {
                query = query.Where(r => r.Vehicle != null && r.Vehicle.Category == vm.Category);
            }

            var reservations = await query.OrderByDescending(r => r.StartDate).ToListAsync();

            vm.Results = reservations.Select(r => new ReportLineItem
            {
                ReservationId = r.ReservationId,
                CustomerName = r.Customer?.FullName ?? "Unknown",
                VehicleName = r.Vehicle != null ? $"{r.Vehicle.Year} {r.Vehicle.Make} {r.Vehicle.Model}" : "Unknown",
                RentalDays = Math.Max(1, (r.EndDate - r.StartDate).Days),
                TotalBilled = r.Billing?.TotalAmount ?? 0
            }).ToList();

            vm.TotalRevenue = vm.Results.Sum(r => r.TotalBilled);

            // Most rented vehicle
            if (reservations.Any())
            {
                var mostRented = reservations
                    .Where(r => r.Vehicle != null)
                    .GroupBy(r => r.VehicleId)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (mostRented != null)
                {
                    var vehicle = mostRented.First().Vehicle;
                    vm.MostRentedVehicle = vehicle != null
                        ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model} ({mostRented.Count()} rentals)"
                        : "N/A";
                }

                // Top customer
                var topCustomer = reservations
                    .Where(r => r.Customer != null)
                    .GroupBy(r => r.CustomerId)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (topCustomer != null)
                {
                    var customer = topCustomer.First().Customer;
                    vm.TopCustomer = customer != null
                        ? $"{customer.FullName} ({topCustomer.Count()} reservations)"
                        : "N/A";
                }
            }

            return View(vm);
        }
    }
}
