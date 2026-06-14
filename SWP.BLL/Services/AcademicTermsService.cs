using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services
{
    public class AcademicTermsService : IAcademicTermsService
    {
        private readonly FlippedClassroomContext _context;

        public AcademicTermsService(FlippedClassroomContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AcademicTerm>> GetAllAsync()
        {
            return await _context.AcademicTerms.OrderByDescending(t => t.StartDate).ToListAsync();
        }

        public async Task<AcademicTerm?> GetByIdAsync(Guid id)
        {
            return await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<AcademicTerm> CreateAsync(AcademicTerm term)
        {
            term.CreatedAt = DateTime.Now;
            _context.AcademicTerms.Add(term);
            await _context.SaveChangesAsync();
            return term;
        }

        public async Task<AcademicTerm> UpdateAsync(Guid id, AcademicTerm term)
        {
            var existing = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException("Không tìm thấy học kỳ.");
            existing.Name      = term.Name;
            existing.TermCode  = term.TermCode;
            existing.StartDate = term.StartDate;
            existing.EndDate   = term.EndDate;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var term = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id);
            if (term == null) return false;
            _context.AcademicTerms.Remove(term);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
