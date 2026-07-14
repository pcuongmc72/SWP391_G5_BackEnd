using System;

namespace SWP.BLL.DTOs.Classes
{
    public class ClassSessionDto
    {
        public Guid Id { get; set; }

        public string ClassId { get; set; } = null!;

        public int WeekNumber { get; set; }

        public string Title { get; set; } = null!;

        public DateOnly SessionDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public string? Description { get; set; }

        public string? Room { get; set; }
    }
}
