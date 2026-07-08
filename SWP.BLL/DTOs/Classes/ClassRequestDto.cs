using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.Classes
{
    public class ClassRequestDto
    {
        [Required(ErrorMessage = "Mã lớp là bắt buộc.")]
        [MaxLength(20)]
        [RegularExpression(@"^[A-Z]{2}\d{4}$", ErrorMessage = "Mã lớp phải bắt đầu bằng 2 chữ cái in hoa và theo sau là đúng 4 chữ số (VD: SE1908).")]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        public Guid CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học kỳ.")]
        public Guid AcademicTermId { get; set; }

        [MaxLength(20)]
        public string? LecturerId { get; set; }

        public bool AllowReviewAfterEnd { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
