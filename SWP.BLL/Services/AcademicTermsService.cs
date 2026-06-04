using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.AcademicTerms;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class AcademicTermsService : IAcademicTermsService
{
    private readonly FlippedClassroomContext _context;

    public AcademicTermsService(FlippedClassroomContext context)
    {
        _context = context;
    }

        public async Task<IEnumerable<AcademicTermResponseDto>> GetAllTermsAsync()
    {
        // Sử dụng Select trực tiếp để chuyển đổi thành câu lệnh SQL COALESCE xử lý Null
        return await _context.AcademicTerms
            .OrderByDescending(t => t.StartDate)
            .Select(t => new AcademicTermResponseDto
            {
                Id = t.Id,
                TermCode = t.TermCode ?? "N/A", // Nếu TermCode trong DB bị NULL thì thay bằng "N/A"
                Name = t.Name ?? "N/A",         // Nếu Name trong DB bị NULL thì thay bằng "N/A"
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }


        public async Task<AcademicTermResponseDto> GetTermByIdAsync(Guid id)
    {
        var term = await _context.AcademicTerms
            .Where(t => t.Id == id)
            .Select(t => new AcademicTermResponseDto
            {
                Id = t.Id,
                TermCode = t.TermCode ?? "N/A",
                Name = t.Name ?? "N/A",
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (term == null)
            throw new KeyNotFoundException("Không tìm thấy học kỳ yêu cầu.");

        return term;
    }


    public async Task<AcademicTermResponseDto> CreateTermAsync(AcademicTermRequestDto request)
    {
        // Kiểm tra trùng lặp mã học kỳ (ví dụ trùng mã SP26)
        bool codeExists = await _context.AcademicTerms
            .AnyAsync(t => t.TermCode != null && t.TermCode.ToLower() == request.TermCode.ToLower());

        if (codeExists)
            throw new InvalidOperationException($"Mã học kỳ '{request.TermCode.ToUpper()}' đã tồn tại trên hệ thống.");

        var newTerm = new AcademicTerm
        {
            // Không gán Id và CreatedAt để SQL Server tự sinh qua thuộc tính DEFAULT
            TermCode = request.TermCode.ToUpper(),
            Name = request.Name,
            StartDate = request.StartDate, 
            EndDate = request.EndDate
        };

        _context.AcademicTerms.Add(newTerm);
        await _context.SaveChangesAsync();

        // EF Core tự động nạp lại Id và CreatedAt từ DB điền vào đối tượng newTerm sau khi Save thành công
        return MapToDto(newTerm);
    }

    public async Task<AcademicTermResponseDto> UpdateTermAsync(Guid id, AcademicTermRequestDto request)
    {
        var term = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id);
        if (term == null)
            throw new KeyNotFoundException("Không tìm thấy học kỳ cần cập nhật.");

        // Kiểm tra xem mã mới định thay đổi có trùng với một học kỳ khác không
        bool codeDuplicate = await _context.AcademicTerms
            .AnyAsync(t => t.Id != id && t.TermCode != null && t.TermCode.ToLower() == request.TermCode.ToLower());

        if (codeDuplicate)
            throw new InvalidOperationException($"Mã học kỳ '{request.TermCode.ToUpper()}' đã được sử dụng bởi học kỳ khác.");

        // Kiểm tra xem thời gian mới có hợp lệ với các lớp học hiện có trong kỳ hay không
        var classes = await _context.Classes
            .Where(c => c.AcademicTermId == id)
            .ToListAsync();

        foreach (var cls in classes)
        {
            if (cls.StartDate.HasValue && cls.StartDate.Value < request.StartDate)
            {
                throw new InvalidOperationException($"Không thể cập nhật! Lớp học '{cls.Id}' có ngày khai giảng ({cls.StartDate:dd/MM/yyyy}) trước ngày khai giảng của học kỳ ({request.StartDate:dd/MM/yyyy}).");
            }
            if (cls.EndDate.HasValue && cls.EndDate.Value > request.EndDate)
            {
                throw new InvalidOperationException($"Không thể cập nhật! Lớp học '{cls.Id}' có ngày bế giảng ({cls.EndDate:dd/MM/yyyy}) sau ngày bế giảng của học kỳ ({request.EndDate:dd/MM/yyyy}).");
            }
        }

        term.TermCode = request.TermCode.ToUpper();
        term.Name = request.Name;
        term.StartDate = request.StartDate;
        term.EndDate = request.EndDate;

        await _context.SaveChangesAsync();
        return MapToDto(term);
    }

    public async Task<bool> DeleteTermAsync(Guid id)
    {
        var term = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id);
        if (term == null)
            throw new KeyNotFoundException("Không tìm thấy học kỳ cần xóa.");

        // Kiểm tra ràng buộc khóa ngoại: Nếu đã có lớp học gán vào kỳ này thì không cho phép xóa
        bool hasClasses = await _context.Classes.AnyAsync(c => c.AcademicTermId == id);
        if (hasClasses)
            throw new InvalidOperationException("Không thể xóa học kỳ này vì đang có lớp học tham chiếu tới.");

        _context.AcademicTerms.Remove(term);
        await _context.SaveChangesAsync();
        return true;
    }

    private static AcademicTermResponseDto MapToDto(AcademicTerm term) => new()
    {
        Id = term.Id,
        TermCode = term.TermCode ?? "N/A",
        Name = term.Name,
        StartDate = term.StartDate,
        EndDate = term.EndDate,
        CreatedAt = term.CreatedAt
    };
}