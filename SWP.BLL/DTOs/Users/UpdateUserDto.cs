using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Ho va ten la bat buoc.")]
        [MaxLength(255)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email la bat buoc.")]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [MaxLength(255)]
        public string Email { get; set; } = null!;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MinLength(6, ErrorMessage = "Password phai co it nhat 6 ky tu.")]
        public string? Password { get; set; } = null!;

        [Required(ErrorMessage = "Trang thai hoat dong la bat buoc.")]
        public bool IsActive { get; set; }
    }
}
