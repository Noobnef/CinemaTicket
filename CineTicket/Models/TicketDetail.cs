using CineTicket.Models;

public class TicketDetail
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; }

    public int SeatId { get; set; }
    public Seat Seat { get; set; }
}
