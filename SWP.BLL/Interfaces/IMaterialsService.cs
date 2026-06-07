using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.Materials;

namespace SWP.BLL.Interfaces;

public interface IMaterialsService
{
    Task<IEnumerable<MaterialDto>> GetMaterialsByClassAsync(string classId, string studentId);
    Task<bool> MarkMaterialCompleteAsync(string materialId, string studentId);
}
