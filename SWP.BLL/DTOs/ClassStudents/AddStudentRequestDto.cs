using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.ClassStudents
{
    public class AddStudentRequestDto
    {
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        [MaxLength(20)]
        public string StudentId { get; set; } = null!;
    }
}
