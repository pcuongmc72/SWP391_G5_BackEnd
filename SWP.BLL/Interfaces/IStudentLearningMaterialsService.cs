using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.LearningMaterials;

namespace SWP.BLL.Interfaces;

public interface IStudentLearningMaterialsService
{
    /// <summary>Lấy danh sách học liệu của lớp kèm trạng thái hoàn thành của sinh viên.</summary>
    Task<IEnumerable<StudentLearningMaterialDto>> GetMaterialsForStudentAsync(string classId, string studentId);

    /// <summary>Đánh dấu sinh viên đã hoàn thành học liệu.</summary>
    Task MarkAsCompletedAsync(Guid materialId, string studentId);

    /// <summary>Bỏ đánh dấu hoàn thành học liệu.</summary>
    Task UnmarkAsCompletedAsync(Guid materialId, string studentId);
}
