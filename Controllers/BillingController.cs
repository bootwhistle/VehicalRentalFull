using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalFull.Data;

namespace VehicleRentalFull.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var billings = await _context.Billings
                .Include(b => b.Reservation)
                    .ThenInclude(r => r!.Customer)
                .Include(b => b.Reservation)
                    .ThenInclude(r => r!.Vehicle)
                .OrderByDescending(b => b.BillingId)
                .ToListAsync();

            return View(billings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var billing = await _context.Billings
                .Include(b => b.Reservation)
                    .ThenInclude(r => r!.Customer)
                .Include(b => b.Reservation)
                    .ThenInclude(r => r!.Vehicle)
                .FirstOrDefaultAsync(b => b.BillingId == id);

            if (billing == null)
            {
                return NotFound();
            }

            return View(billing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null)
            {
                return NotFound();
            }

            billing.IsPaid = true;
            billing.PaidAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment recorded successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            var billing = await _context.Billings
                .Include(b => b.Reservation)
                    .ThenInclude(r => r!.Customer)
                .Include(b => b.Reservation)
                    .ThenInclude(r => r!.Vehicle)
                .FirstOrDefaultAsync(b => b.BillingId == id);

            if (billing == null)
            {
                return NotFound();
            }

            return View(billing);
        }
    }
}
