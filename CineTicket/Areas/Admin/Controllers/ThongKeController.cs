using CineTicket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Tickets
                .Where(t => t.BookingTime != null)
                .GroupBy(t => t.BookingTime.Date)
                .Select(g => new ThongKeDoanhThu
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(x => x.Price),
                    TicketsSold = g.Count()
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return View(data);
        }
    }
}
