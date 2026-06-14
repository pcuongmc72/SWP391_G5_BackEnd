using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services
{
    public class ClassStudentsService : IClassStudentsService
    {
        private readonly FlippedClassroomContext _context;

        public ClassStudentsService(FlippedClassroomContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassStudent>> GetByClassIdAsync(string classId)
        {
            return await _context.ClassStudents
                .Include(cs => cs.Student)
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();
        }

        public async Task<ClassStudent> EnrollAsync(string classId, string studentId)
        {
            var exists = await _context.ClassStudents
                .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
            if (exists)
                throw new InvalidOperationException("Sinh viên đã được đăng ký vào lớp học này.");

            var enrollment = new ClassStudent
            {
                ClassId   = classId,
                StudentId = studentId,
                EnrolledAt = DateTime.Now
            };
            _context.ClassStudents.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<bool> UnenrollAsync(string classId, string studentId)
        {
            var enrollment = await _context.ClassStudents
                .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
            if (enrollment == null) return false;
            _context.ClassStudents.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
