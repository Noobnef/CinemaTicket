namespace CineTicket.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int SeatNumber { get; set; }         
        public string Row { get; set; }            

        public int RoomId { get; set; }
        public Room Room { get; set; }
    }

}
