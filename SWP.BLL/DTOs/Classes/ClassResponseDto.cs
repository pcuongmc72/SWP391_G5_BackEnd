using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.Classes
{
    public class ClassResponseDto
    {
        public string Id { get; set; } = null!; 
        public string Name { get; set; } = null!; 

        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = null!; 
        public string CourseName { get; set; } = null!; 

        public Guid AcademicTermId { get; set; }
        public string TermCode { get; set; } = null!; 

        public string? LecturerId { get; set; }
        public string LecturerName { get; set; } = null!; 

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public int TotalStudents { get; set; }
        public bool AllowReviewAfterEnd { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
