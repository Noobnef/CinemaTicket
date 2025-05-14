using CineTicket.Models;

public class ChooseShowtimeViewModel
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; }
    public List<Showtime> Showtimes { get; set; }
}
