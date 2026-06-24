using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.ClassStudents
{
    public class StudentInClassDto
    {
        public string StudentId { get; set; } = null!; 
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
