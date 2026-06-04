using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.DTOs.AcademicTerms
{
    public class AcademicTermResponseDto
    {
        public Guid Id { get; set; }
        public string TermCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
