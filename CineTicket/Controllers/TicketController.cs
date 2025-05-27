using CineTicket.Models;
using Microsoft.AspNetCore.Mvc;
using CineTicket.Repositories; 
public class TicketController : Controller
{
    private readonly ITicketRepository _ticketRepository;

    public TicketController(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public IActionResult Book(int showtimeId)
    {
        var showtime = _ticketRepository.GetShowtime(showtimeId);
        if (showtime == null) return NotFound();

        return View(showtime);
    }

    [HttpPost]
    public IActionResult Book(int showtimeId, string seatNumber)
    {
        var ticket = new Ticket { ShowtimeId = showtimeId, SeatNumber = seatNumber };
        _ticketRepository.AddTicket(ticket);
        _ticketRepository.SaveChanges();

        return RedirectToAction("Index", "Movie");
    }
}
