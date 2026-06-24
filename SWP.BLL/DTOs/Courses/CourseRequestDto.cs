using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.Courses
{
    public class CourseRequestDto
    {
        [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mã môn học chỉ được chứa chữ và số, viết liền (VD: PRN232).")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Tên môn học là bắt buộc.")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
