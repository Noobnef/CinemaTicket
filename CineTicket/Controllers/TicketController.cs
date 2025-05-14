using CineTicket.Models;
using Microsoft.AspNetCore.Mvc;


public class TicketController : Controller
{
    private readonly ApplicationDbContext _context;

    public TicketController(ApplicationDbContext context) { _context = context; }

    public IActionResult Book(int showtimeId)
    {
        var showtime = _context.Showtimes.FirstOrDefault(s => s.Id == showtimeId);
        if (showtime == null) return NotFound();

        return View(showtime);
    }

    [HttpPost]
    public IActionResult Book(int showtimeId, string seatNumber)
    {
        var ticket = new Ticket { ShowtimeId = showtimeId, SeatNumber = seatNumber };
        _context.Tickets.Add(ticket);
        _context.SaveChanges();

        return RedirectToAction("Index", "Movie");
    }


}
