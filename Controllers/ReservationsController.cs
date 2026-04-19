using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleRentalFull.Data;
using VehicleRentalFull.Models;
using VehicleRentalFull.Models.ViewModels;

namespace VehicleRentalFull.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ReservationCreateViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Customers = new SelectList(
                    await _context.Customers.OrderBy(c => c.FullName).ToListAsync(),
                    "CustomerId", "FullName"),
                Vehicles = new SelectList(
                    await _context.Vehicles
                        .Where(v => v.IsAvailable)
                        .OrderBy(v => v.Make)
                        .Select(v => new { v.VehicleId, Name = $"{v.Year} {v.Make} {v.Model} - ${v.DailyRate}/day ({v.LicensePlate})" })
                        .ToListAsync(),
                    "VehicleId", "Name")
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateSelectLists(vm);
                return View(vm);
            }

            if (vm.EndDate <= vm.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                await RepopulateSelectLists(vm);
                return View(vm);
            }

            // Check for date conflicts
            var conflict = await _context.Reservations
                .AnyAsync(r => r.VehicleId == vm.VehicleId
                    && r.Status != "Cancelled"
                    && r.StartDate < vm.EndDate
                    && r.EndDate > vm.StartDate);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "This vehicle is already reserved for the selected dates. Please choose different dates or a different vehicle.");
                await RepopulateSelectLists(vm);
                return View(vm);
            }

            var reservation = new Reservation
            {
                CustomerId = vm.CustomerId,
                VehicleId = vm.VehicleId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Status = string.IsNullOrEmpty(vm.Status) ? "Confirmed" : vm.Status,
                CreatedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);

            // Mark vehicle as unavailable
            var vehicle = await _context.Vehicles.FindAsync(vm.VehicleId);
            if (vehicle != null)
            {
                vehicle.IsAvailable = false;
            }

            await _context.SaveChangesAsync();

            // Create billing record
            int rentalDays = Math.Max(1, (vm.EndDate - vm.StartDate).Days);
            decimal dailyRate = vehicle?.DailyRate ?? 0;
            decimal baseCost = rentalDays * dailyRate;
            decimal taxAmount = baseCost * 0.10m;
            decimal totalAmount = baseCost + taxAmount;

            var billing = new Billing
            {
                ReservationId = reservation.ReservationId,
                BaseCost = baseCost,
                TaxAmount = taxAmount,
                AdditionalCharges = 0,
                TotalAmount = totalAmount,
                IsPaid = false
            };

            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var vm = new ReservationCreateViewModel
            {
                CustomerId = reservation.CustomerId,
                VehicleId = reservation.VehicleId,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Status = reservation.Status,
                Customers = new SelectList(
                    await _context.Customers.OrderBy(c => c.FullName).ToListAsync(),
                    "CustomerId", "FullName", reservation.CustomerId),
                Vehicles = new SelectList(
                    await _context.Vehicles
                        .OrderBy(v => v.Make)
                        .Select(v => new { v.VehicleId, Name = $"{v.Year} {v.Make} {v.Model} - ${v.DailyRate}/day ({v.LicensePlate})" })
                        .ToListAsync(),
                    "VehicleId", "Name", reservation.VehicleId)
            };

            ViewBag.ReservationId = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReservationCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateSelectListsForEdit(vm, id);
                ViewBag.ReservationId = id;
                return View(vm);
            }

            if (vm.EndDate <= vm.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                await RepopulateSelectListsForEdit(vm, id);
                ViewBag.ReservationId = id;
                return View(vm);
            }

            // Check for conflicts excluding current reservation
            var conflict = await _context.Reservations
                .AnyAsync(r => r.VehicleId == vm.VehicleId
                    && r.ReservationId != id
                    && r.Status != "Cancelled"
                    && r.StartDate < vm.EndDate
                    && r.EndDate > vm.StartDate);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "This vehicle is already reserved for the selected dates.");
                await RepopulateSelectListsForEdit(vm, id);
                ViewBag.ReservationId = id;
                return View(vm);
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // If vehicle changed, update availability
            if (reservation.VehicleId != vm.VehicleId)
            {
                var oldVehicle = await _context.Vehicles.FindAsync(reservation.VehicleId);
                if (oldVehicle != null) oldVehicle.IsAvailable = true;

                var newVehicle = await _context.Vehicles.FindAsync(vm.VehicleId);
                if (newVehicle != null) newVehicle.IsAvailable = false;
            }

            reservation.CustomerId = vm.CustomerId;
            reservation.VehicleId = vm.VehicleId;
            reservation.StartDate = vm.StartDate;
            reservation.EndDate = vm.EndDate;
            reservation.Status = vm.Status;

            // Recalculate billing
            var vehicle = await _context.Vehicles.FindAsync(vm.VehicleId);
            var billing = await _context.Billings.FirstOrDefaultAsync(b => b.ReservationId == id);
            if (billing != null && vehicle != null)
            {
                int rentalDays = Math.Max(1, (vm.EndDate - vm.StartDate).Days);
                billing.BaseCost = rentalDays * vehicle.DailyRate;
                billing.TaxAmount = billing.BaseCost * 0.10m;
                billing.TotalAmount = billing.BaseCost + billing.TaxAmount + billing.AdditionalCharges;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Reservation updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Cancelled";

            if (reservation.Vehicle != null)
            {
                reservation.Vehicle.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Reservation cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task RepopulateSelectLists(ReservationCreateViewModel vm)
        {
            vm.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.FullName).ToListAsync(),
                "CustomerId", "FullName", vm.CustomerId);
            vm.Vehicles = new SelectList(
                await _context.Vehicles
                    .Where(v => v.IsAvailable)
                    .OrderBy(v => v.Make)
                    .Select(v => new { v.VehicleId, Name = $"{v.Year} {v.Make} {v.Model} - ${v.DailyRate}/day ({v.LicensePlate})" })
                    .ToListAsync(),
                "VehicleId", "Name", vm.VehicleId);
        }

        private async Task RepopulateSelectListsForEdit(ReservationCreateViewModel vm, int reservationId)
        {
            vm.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.FullName).ToListAsync(),
                "CustomerId", "FullName", vm.CustomerId);
            vm.Vehicles = new SelectList(
                await _context.Vehicles
                    .OrderBy(v => v.Make)
                    .Select(v => new { v.VehicleId, Name = $"{v.Year} {v.Make} {v.Model} - ${v.DailyRate}/day ({v.LicensePlate})" })
                    .ToListAsync(),
                "VehicleId", "Name", vm.VehicleId);
        }
    }
}
