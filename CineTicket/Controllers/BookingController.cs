using Microsoft.AspNetCore.Mvc;
using CineTicket.Models;
using Microsoft.AspNetCore.Identity;
using CineTicket.Repositories;
using CineTicket.ViewModels;
using Microsoft.AspNetCore.Authorization;
using CineTicket.Models.ViewModels;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using CineTicket.Payment;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

public class BookingController : Controller
{
    private readonly IBookingRepository _bookingRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGmailSender _gmailSender;

    public BookingController(
        IBookingRepository bookingRepo,
        UserManager<ApplicationUser> userManager,
        IGmailSender gmailSender)
    {
        _bookingRepo = bookingRepo;
        _userManager = userManager;
        _gmailSender = gmailSender;
    }

    // Chọn suất chiếu
    public IActionResult Index(int movieId, int? showtimeId)
    {
        var movie = _bookingRepo.GetMovie(movieId);
        if (movie == null)
            return NotFound("Không tìm thấy phim.");

        if (!showtimeId.HasValue)
        {
            var showtimes = _bookingRepo.GetShowtimes(movieId);

            if (!showtimes.Any())
                return BadRequest("Phim này chưa có suất chiếu.");

            return View("ChooseShowtime", new ChooseShowtimeViewModel
            {
                MovieId = movie.Id,
                MovieTitle = movie.Title,
                Showtimes = showtimes
            });
        }

        var selectedShowtime = _bookingRepo.GetShowtimeWithRoom(showtimeId.Value, movieId);

        if (selectedShowtime == null)
            return BadRequest("Suất chiếu không hợp lệ.");

        var bookedSeats = _bookingRepo.GetBookedSeats(selectedShowtime.Id);

        var booking = new BookingViewModel
        {
            MovieId = movie.Id,
            MovieTitle = movie.Title,
            TicketPrice = selectedShowtime.Room.TicketPrice,
            ShowtimeId = selectedShowtime.Id,
            AlreadyBookedSeats = bookedSeats,
            AvailableSnacks = _bookingRepo.GetSnacks()
        };

        return View(booking);
    }

    // Xác nhận đặt vé
    [HttpPost]
    public async Task<IActionResult> ConfirmBooking(BookingViewModel model)
    {
        if (string.IsNullOrEmpty(model.SeatNumbers))
            return RedirectToAction("Index", new { movieId = model.MovieId, showtimeId = model.ShowtimeId });

        var seats = model.SeatNumbers.Split(',');
        var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null;
        decimal totalPrice = model.TicketPrice * seats.Length;

        var tickets = seats.Select(seat => new Ticket
        {
            ShowtimeId = model.ShowtimeId,
            SeatNumber = seat,
            Price = model.TicketPrice,
            BookingTime = DateTime.Now,
            UserId = userId
        }).ToList();

        await _bookingRepo.AddTicketsAsync(tickets);

        var snackOrders = new List<SnackOrder>();
        if (model.SelectedSnackIds != null && model.SelectedSnackIds.Any())
        {
            foreach (var snackId in model.SelectedSnackIds)
            {
                var snack = await _bookingRepo.FindSnackAsync(snackId);
                if (snack != null)
                {
                    snackOrders.Add(new SnackOrder
                    {
                        SnackId = snack.Id,
                        Quantity = 1,
                        Price = snack.Price,
                        BookingTime = DateTime.Now,
                        UserId = userId,
                        ShowtimeId = model.ShowtimeId
                    });
                    totalPrice += snack.Price;
                }
            }
            await _bookingRepo.AddSnackOrdersAsync(snackOrders);
        }

        // Lưu vào lịch sử đặt vé
        var history = new BookingHistory
        {
            UserId = userId,
            ShowtimeId = model.ShowtimeId,
            SeatNumbers = model.SeatNumbers,
            TotalAmount = totalPrice,
            BookingDate = DateTime.Now,
            IsPaid = false // chưa thanh toán
        };
        await _bookingRepo.AddBookingHistoryAsync(history);
        await _bookingRepo.SaveChangesAsync(); // Đảm bảo history.Id đã sinh ra

        // Redirect sang Cart với bookingId vừa lưu
        return RedirectToAction("Cart", new { bookingId = history.Id });
    }

    // Giỏ hàng - chỉ lấy đúng bookingId vừa đặt hoặc orderId khi vừa thanh toán xong
    [Authorize]
    public async Task<IActionResult> Cart(int? bookingId = null, string orderId = null)
    {
        var userId = _userManager.GetUserId(User);
        BookingHistory latestHistory = null;

        if (!string.IsNullOrEmpty(orderId))
        {
            latestHistory = await _bookingRepo.GetBookingHistoryByOrderIdAsync(orderId, userId);
        }
        else if (bookingId.HasValue)
        {
            latestHistory = await _bookingRepo.GetBookingHistoryByIdAsync(bookingId.Value, userId);
        }
        else
        {
            latestHistory = await _bookingRepo.GetLatestBookingHistoryAsync(userId);
        }

        if (latestHistory == null)
        {
            return View(new List<CartItemViewModel>());
        }

        var snackNames = await _bookingRepo.GetSnackNamesForHistoryAsync(userId, latestHistory.ShowtimeId, latestHistory.BookingDate);

        var cartItem = new CartItemViewModel
        {
            Id = latestHistory.Id,
            BookingId = latestHistory.Id,
            MovieTitle = latestHistory.Showtime?.Movie?.Title,
            Showtime = latestHistory.Showtime?.StartTime ?? DateTime.MinValue,
            SeatNumbers = latestHistory.SeatNumbers,
            TotalAmount = latestHistory.TotalAmount,
            BookingDate = latestHistory.BookingDate,
            SnackNames = snackNames,
            IsPaid = latestHistory.IsPaid
        };

        return View(new List<CartItemViewModel> { cartItem });
    }

    // ========================== XỬ LÝ MOMO =======================================
    [Authorize]
    public async Task<IActionResult> Momo(int bookingId)
    {
        var userId = _userManager.GetUserId(User);
        var history = await _bookingRepo.GetBookingHistoryByIdAsync(bookingId, userId);

        if (history == null || history.IsPaid)
        {
            return BadRequest("Không tìm thấy đơn hợp lệ hoặc đã thanh toán.");
        }

        string amount = ((int)history.TotalAmount).ToString();
        string orderid = $"{history.Id}_{DateTime.Now.Ticks}";
        history.OrderId = orderid;  // Lưu OrderId vào DB
        await _bookingRepo.SaveChangesAsync();

        string requestId = DateTime.Now.Ticks.ToString();
        string extraData = "";

        string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
        string partnerCode = "MOMOOJOI20210710";
        string accessKey = "iPXneGmrJH0G8FOP";
        string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
        string orderInfo = $"Thanh toán vé xem phim - Đơn {orderid}";
        string domain = $"{Request.Scheme}://{Request.Host}";
        string returnUrl = $"{domain}/Booking/Success";
        string notifyurl = $"{domain}/Booking/MomoSavePayment";

        string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderid}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyurl}&extraData={extraData}";

        MoMoSecurity crypto = new MoMoSecurity();
        string signature = crypto.signSHA256(rawHash, serectkey);

        JObject message = new JObject
        {
            { "partnerCode", partnerCode },
            { "accessKey", accessKey },
            { "requestId", requestId },
            { "amount", amount },
            { "orderId", orderid },
            { "orderInfo", orderInfo },
            { "returnUrl", returnUrl },
            { "notifyUrl", notifyurl },
            { "extraData", extraData },
            { "requestType", "captureMoMoWallet" },
            { "signature", signature }
        };

        string responseFromMomo = MomoPaymentRequest.sendPaymentRequest(endpoint, message.ToString());
        JObject jmessage = JObject.Parse(responseFromMomo);

        if (jmessage["payUrl"] == null || string.IsNullOrEmpty(jmessage["payUrl"].ToString()))
        {
            System.Diagnostics.Debug.WriteLine("MoMo trả về lỗi: " + responseFromMomo);
            return Content("Lỗi MoMo (không có payUrl): <pre>" + responseFromMomo + "</pre>");
        }

        return Redirect(jmessage.GetValue("payUrl").ToString());
    }

    // Callback server to server từ Momo (notifyUrl)
    [HttpPost]
    public async Task<IActionResult> MomoSavePayment()
    {
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var body = await reader.ReadToEndAsync();
            var json = JObject.Parse(body);

            string orderId = json["orderId"]?.ToString();
            string errorCode = json["errorCode"]?.ToString();

            if (errorCode == "0")
            {
                await _bookingRepo.UpdateBookingHistoryPaidAsync(orderId);
            }

            return Ok(new { message = "Payment received" });
        }
    }

    public IActionResult Success()
    {
        return View();
    }



}
