using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.AcademicTerms;

public class AcademicTermRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Mã học kỳ là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "Mã học kỳ không được quá 20 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mã học kỳ chỉ được chứa chữ và số, viết liền không dấu (VD: SP26).")]
    public string TermCode { get; set; } = null!;

    [Required(ErrorMessage = "Tên học kỳ là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Tên học kỳ không được quá 255 ký tự.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Ngày khai giảng là bắt buộc.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "Ngày bế giảng là bắt buộc.")]
    public DateOnly EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Vì là DateOnly rồi nên không cần gọi .Date ở đây nữa
        if (EndDate < StartDate)
        {
            yield return new ValidationResult(
                "Ngày bế giảng phải lớn hơn hoặc bằng ngày khai giảng.",
                new[] { nameof(EndDate) }
            );
        }
    }
}