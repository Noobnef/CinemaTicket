using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CineTicket.Models
{
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public List<RoleSelectionViewModel> Roles { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn một quyền.")]
        public string SelectedRole { get; set; }

    }

    public class RoleSelectionViewModel
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
