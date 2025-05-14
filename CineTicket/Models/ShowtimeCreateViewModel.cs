using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CineTicket.ViewModels
{
    public class ShowtimeCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn thời gian chiếu.")]
        [Display(Name = "Thời gian chiếu")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phim.")]
        [Display(Name = "Phim")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng.")]
        [Display(Name = "Phòng chiếu")]
        public int RoomId { get; set; }

        public IEnumerable<SelectListItem> Movies { get; set; }
        public IEnumerable<SelectListItem> Rooms { get; set; }
    }
}
