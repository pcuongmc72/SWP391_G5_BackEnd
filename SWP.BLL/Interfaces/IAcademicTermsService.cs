using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.AcademicTerms;

namespace SWP.BLL.Interfaces;

public interface IAcademicTermsService
{
    Task<IEnumerable<AcademicTermResponseDto>> GetAllTermsAsync();
    Task<AcademicTermResponseDto> GetTermByIdAsync(Guid id);
    Task<AcademicTermResponseDto> CreateTermAsync(AcademicTermRequestDto request);
    Task<AcademicTermResponseDto> UpdateTermAsync(Guid id, AcademicTermRequestDto request);
    Task<bool> DeleteTermAsync(Guid id);
}