using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalFull.Data;
using VehicleRentalFull.Models;

namespace VehicleRentalFull.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Vehicles.AsQueryable();

            if (status == "available")
            {
                query = query.Where(v => v.IsAvailable);
            }
            else if (status == "rented")
            {
                query = query.Where(v => !v.IsAvailable);
            }

            ViewBag.CurrentStatus = status;
            var vehicles = await query.OrderBy(v => v.Make).ThenBy(v => v.Model).ToListAsync();
            return View(vehicles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Vehicle());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Vehicle {vehicle.Year} {vehicle.Make} {vehicle.Model} added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle)
        {
            if (id != vehicle.VehicleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Vehicle updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehicles.Any(v => v.VehicleId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(vehicle);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
