using CineTicket.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Poster> Posters { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Snack> Snacks { get; set; }
    public DbSet<SnackOrder> SnackOrder { get; set; }

    public DbSet<Seat> ThongKeDoanhThu { get; set; }
    public DbSet<BookingHistory> BookingHistories { get; set; }

}
