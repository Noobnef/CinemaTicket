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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

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
        {
            return RedirectToAction("Index", new { movieId = model.MovieId, showtimeId = model.ShowtimeId });
        }

        var seats = model.SeatNumbers.Split(',');
        var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null;
        decimal totalPrice = model.TicketPrice * seats.Length;

        // Thêm vé vào cơ sở dữ liệu
        var tickets = seats.Select(seat => new Ticket
        {
            ShowtimeId = model.ShowtimeId,
            SeatNumber = seat,
            Price = model.TicketPrice,
            BookingTime = DateTime.Now,
            UserId = userId
        }).ToList();

        await _bookingRepo.AddTicketsAsync(tickets);

        // Thêm bắp nước
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
            BookingDate = DateTime.Now
        };
        await _bookingRepo.AddBookingHistoryAsync(history);

        await _bookingRepo.SaveChangesAsync();

        // Gửi email xác nhận
        var movie = await _bookingRepo.FindMovieAsync(model.MovieId);
        var pdfBytes = GenerateTicketPdf(movie.Title, model.SeatNumbers, totalPrice);

        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            await _gmailSender.SendEmailWithAttachmentAsync(user.Email, "Vé xem phim CineTicket",
                $"Cảm ơn bạn đã đặt vé cho phim: {movie.Title}. " +
                $"Ghế: {model.SeatNumbers}. " +
                $"Tổng tiền: {totalPrice:N0} VND.",
                pdfBytes, "ve-phim.pdf");
        }

        return RedirectToAction("Cart");
    }

    // Giỏ hàng
    [Authorize]
    public async Task<IActionResult> Cart()
    {
        var userId = _userManager.GetUserId(User);

        var latestHistory = await _bookingRepo.GetLatestBookingHistoryAsync(userId);

        if (latestHistory == null)
        {
            return View(new List<CartItemViewModel>());
        }

        var snackNames = await _bookingRepo.GetSnackNamesForHistoryAsync(userId, latestHistory.ShowtimeId, latestHistory.BookingDate);

        var cartItem = new CartItemViewModel
        {
            MovieTitle = latestHistory.Showtime.Movie.Title,
            Showtime = latestHistory.Showtime.StartTime,
            SeatNumbers = latestHistory.SeatNumbers,
            TotalAmount = latestHistory.TotalAmount,
            BookingDate = latestHistory.BookingDate,
            SnackNames = snackNames
        };

        return View(new List<CartItemViewModel> { cartItem });
    }

    // Trang thành công
    public IActionResult Success()
    {
        return View();
    }

    // Tạo PDF vé
    private byte[] GenerateTicketPdf(string movie, string seats, decimal amount)
    {
        using var ms = new MemoryStream();
        var doc = new PdfDocument();
        var page = doc.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var font = new XFont("Arial", 14);
        var brush = XBrushes.Black;

        string content = $"\uD83C\uDFAC Vé Xem Phim\n" +
                         $"Phim: {movie}\n" +
                         $"Ghế: {seats}\n" +
                         $"Tổng tiền: {amount:N0} VND\n" +
                         $"Ngày đặt: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var rect = new XRect(40, 40, page.Width - 80, page.Height - 80);
        gfx.DrawString(content, font, brush, rect, XStringFormats.TopLeft);

        doc.Save(ms);
        return ms.ToArray();
    }

    public ActionResult Momo()
    {

        //request params need to request to MoMo system
        string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
        string partnerCode = "MOMOOJOI20210710";
        string accessKey = "iPXneGmrJH0G8FOP";
        string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
        string orderInfo = "Thanh toan Cinema Ticket Hub";
        string returnUrl =  "/Booking/MomoConfirmed";
        string notifyurl =  "/Booking/MomoSavePayment";

        //Lấy tổng tiền trong giỏ hàng

        string amount = "100000";
        string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
        string requestId = DateTime.Now.Ticks.ToString();
        string extraData = "";

        //Before sign HMAC SHA256 signature
        string rawHash = "partnerCode=" +
            partnerCode + "&accessKey=" +
            accessKey + "&requestId=" +
            requestId + "&amount=" +
            amount + "&orderId=" +
            orderid + "&orderInfo=" +
            orderInfo + "&returnUrl=" +
            returnUrl + "&notifyUrl=" +
            notifyurl + "&extraData=" +
            extraData;

        MoMoSecurity crypto = new MoMoSecurity();
        //sign signature SHA256
        string signature = crypto.signSHA256(rawHash, serectkey);

        //build body json request
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

        return Redirect(jmessage.GetValue("payUrl").ToString());
    }

    //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
    //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
    //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
    public ActionResult MomoConfirmed(CineTicket.Payment.MomoResult result)
    {
        //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
        string rMessage = result.message;
        string rOrderId = result.orderId;
        string rErrorCode = result.errorCode; // = 0: thanh toán thành công
        ViewBag.MomoStatus = rErrorCode;
        ViewBag.Message = "Hóa đơn: " + rOrderId;

        if (rErrorCode == "0")
        {
            
        }
        else
        {
           
        }
        return View();
    }

    [HttpPost]
    public void MomoSavePayment()
    {
        //cập nhật dữ liệu vào db
        String a = "";
    }
}
