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
            // Lấy thống kê vé
            var ticketStats = await _context.Tickets
                .Where(t => t.BookingTime != null)
                .GroupBy(t => t.BookingTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TicketRevenue = g.Sum(x => x.Price),
                    TicketsSold = g.Count()
                }).ToListAsync();

            // Lấy thống kê bắp nước
            var snackStats = await _context.SnackOrder
                .GroupBy(s => s.BookingTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    SnackRevenue = g.Sum(x => x.Price * x.Quantity),
                    SnacksSold = g.Sum(x => x.Quantity)
                }).ToListAsync();

            // Gộp hai nguồn vào 1 list, cộng tổng doanh thu
            var allDates = ticketStats.Select(x => x.Date)
                .Union(snackStats.Select(x => x.Date))
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();

            var result = allDates.Select(date =>
            {
                var ticket = ticketStats.FirstOrDefault(x => x.Date == date);
                var snack = snackStats.FirstOrDefault(x => x.Date == date);
                return new ThongKeDoanhThu
                {
                    Date = date,
                    TotalRevenue = (ticket?.TicketRevenue ?? 0) + (snack?.SnackRevenue ?? 0),
                    TicketsSold = ticket?.TicketsSold ?? 0,
                    SnackRevenue = snack?.SnackRevenue ?? 0,
                    SnacksSold = snack?.SnacksSold ?? 0
                };
            }).ToList();

            return View(result);
        }

    }
}
