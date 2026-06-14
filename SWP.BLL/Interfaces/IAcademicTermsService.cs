using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.DAL.Models;

namespace SWP.BLL.Interfaces
{
    public interface IAcademicTermsService
    {
        Task<IEnumerable<AcademicTerm>> GetAllAsync();
        Task<AcademicTerm?> GetByIdAsync(Guid id);
        Task<AcademicTerm> CreateAsync(AcademicTerm term);
        Task<AcademicTerm> UpdateAsync(Guid id, AcademicTerm term);
        Task<bool> DeleteAsync(Guid id);
    }
}
