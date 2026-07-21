using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Blogs;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP.BLL.Services
{
    public class BlogsService : IBlogsService
    {
        private readonly FlippedClassroomContext _context;

        public BlogsService(FlippedClassroomContext context)
        {
            _context = context;
        }

        private static DateTime GetHanoiTime()
        {
            return DateTime.UtcNow.AddHours(7);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetAllBlogsForAdminAsync(Guid? courseId = null)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(b => b.CourseId == courseId.Value);
            }

            var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return blogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetPrivateBlogsAsync(Guid? courseId = null)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .Where(b => b.IsPrivate);

            if (courseId.HasValue)
            {
                query = query.Where(b => b.CourseId == courseId.Value);
            }

            var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return blogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetAllPublicBlogsAsync(Guid? courseId = null, int? status = 1)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .Where(b => b.Status == (status ?? 1) && !b.IsPrivate);

            if (courseId.HasValue)
            {
                query = query.Where(b => b.CourseId == courseId.Value);
            }

            var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return blogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetClassBlogsAsync(string classId, Guid? courseId = null, int? status = 1)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .Where(b => b.ClassId == classId);

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            if (courseId.HasValue)
            {
                query = query.Where(b => b.CourseId == courseId.Value);
            }

            var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return blogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetUserBlogsAsync(string userId)
        {
            var blogs = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .Where(b => b.AuthorId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return blogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetStudentClassBlogsAsync(string studentId, Guid? courseId = null)
        {
            // Get all class IDs and course IDs the student is enrolled in
            var enrollments = await _context.ClassStudents
                .Include(cs => cs.Class)
                .Where(cs => cs.StudentId == studentId)
                .ToListAsync();

            var classIds = enrollments.Select(cs => cs.ClassId).ToList();
            var enrolledCourseIds = enrollments.Select(cs => cs.Class.CourseId).Distinct().ToList();

            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .Where(b => (b.Status == 1 || b.AuthorId == studentId) &&
                            (
                                // 1. Blogs specifically for their classes
                                (b.ClassId != null && classIds.Contains(b.ClassId)) ||
                                // 2. Public blogs for their courses (even if no ClassId)
                                (b.ClassId == null && !b.IsPrivate && enrolledCourseIds.Contains(b.CourseId)) ||
                                // 3. Their own blogs (already covered by AuthorId == studentId, but ensure they see them here)
                                (b.AuthorId == studentId)
                            ));

            if (courseId.HasValue)
            {
                query = query.Where(b => b.CourseId == courseId.Value);
            }

            var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return blogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BlogResponseDto>> GetLecturerClassBlogsAsync(string lecturerId, Guid? courseId = null)
        {
            // Get all class IDs where this lecturer is teaching
            var classIds = await _context.Classes
                .Where(c => c.LecturerId == lecturerId)
                .Select(c => c.Id)
                .ToListAsync();

            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .Where(b => b.ClassId != null && classIds.Contains(b.ClassId));

            if (courseId.HasValue)
            {
                query = query.Where(b => b.CourseId == courseId.Value);
            }

            var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return blogs.Select(MapToResponseDto);
        }



        public async Task<BlogResponseDto> GetBlogByIdAsync(Guid id)
        {
            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) return null!;

            return MapToResponseDto(blog);
        }

        public async Task<BlogResponseDto> CreateBlogAsync(BlogRequestDto request)
        {
            // Validation for private blogs
            if (request.IsPrivate && !string.IsNullOrEmpty(request.ClassId))
            {
                var targetClass = await _context.Classes.FindAsync(request.ClassId);
                if (targetClass == null) throw new KeyNotFoundException("Class not found.");

                bool isAllowed = false;

                // 1. Is the author the lecturer of the class?
                if (targetClass.LecturerId == request.AuthorId)
                {
                    isAllowed = true;
                }
                // 2. Is the author a student in the class?
                else if (await _context.ClassStudents.AnyAsync(cs => cs.ClassId == request.ClassId && cs.StudentId == request.AuthorId))
                {
                    isAllowed = true;
                }
                // 3. Is the author an Admin?
                else
                {
                    var author = await _context.Users.FindAsync(request.AuthorId);
                    if (author != null && author.Role?.ToLower() == "admin")
                    {
                        isAllowed = true;
                    }
                }

                if (!isAllowed)
                {
                    throw new UnauthorizedAccessException("You are not authorized to post a private blog for this class.");
                }
            }

            var blog = new Blog
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId,
                CourseId = request.CourseId,
                ClassId = request.ClassId,
                IsPrivate = request.IsPrivate,
                Keywords = request.Keywords,
                CreatedAt = GetHanoiTime(),
                UpdatedAt = GetHanoiTime()
            };

            // Initial status logic
            if (blog.IsPrivate)
            {
                blog.Status = 1; // Auto-approved for class internal use
            }
            else
            {
                var user = await _context.Users.FindAsync(request.AuthorId);
                if (user != null && (user.Role == "admin" || user.Role == "lecturer"))
                {
                    blog.Status = 1; // Auto-approved for lecturers/admins
                }
                else
                {
                    blog.Status = 0; // Pending for students
                }
            }

            await _context.Blogs.AddAsync(blog);
            await _context.SaveChangesAsync();

            // Reload to get navigation properties
            return await GetBlogByIdAsync(blog.Id);
        }

        public async Task<BlogResponseDto> UpdateBlogAsync(Guid id, BlogRequestDto request)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return null!;

            blog.Title = request.Title;
            blog.Content = request.Content;
            blog.CourseId = request.CourseId;
            blog.ClassId = request.ClassId;
            blog.IsPrivate = request.IsPrivate;
            blog.Keywords = request.Keywords;
            blog.UpdatedAt = GetHanoiTime();

            // Re-evaluate status if it's not private anymore
            if (blog.IsPrivate)
            {
                blog.Status = 1; // Auto-approved for class
            }
            else
            {
                // If it's a student making it public, it needs re-approval
                var user = await _context.Users.FindAsync(blog.AuthorId);
                if (user != null && (user.Role == "admin" || user.Role == "lecturer"))
                {
                    blog.Status = 1;
                }
                else
                {
                    blog.Status = 0; // Back to pending
                }
            }

            await _context.SaveChangesAsync();
            return await GetBlogByIdAsync(id);
        }

        public async Task<bool> ApproveBlogAsync(Guid id, int status)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return false;

            blog.Status = status;
            blog.UpdatedAt = GetHanoiTime();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBlogAsync(Guid id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return false;

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Comments Implementation ---

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByBlogIdAsync(Guid blogId)
        {
            var comments = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.BlogId == blogId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                BlogId = c.BlogId,
                AuthorId = c.AuthorId,
                AuthorFullName = c.Author?.FullName ?? "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Role = c.Author?.Role
            });
        }

        public async Task<CommentResponseDto> CreateCommentAsync(CommentRequestDto request)
        {
            var blog = await _context.Blogs
                .Include(b => b.Class)
                .FirstOrDefaultAsync(b => b.Id == request.BlogId);

            if (blog == null) throw new KeyNotFoundException("Blog not found.");

            // Validation logic
            if (blog.IsPrivate)
            {
                // Private blogs REQUIRE class membership
                if (!string.IsNullOrEmpty(blog.ClassId))
                {
                    bool isAllowed = false;

                    // 1. Is the commenter the lecturer of the class?
                    if (blog.Class?.LecturerId == request.AuthorId) isAllowed = true;
                    // 2. Is the commenter a student in the class?
                    else if (await _context.ClassStudents.AnyAsync(cs => cs.ClassId == blog.ClassId && cs.StudentId == request.AuthorId)) isAllowed = true;
                    // 3. Is the commenter the author of the blog post?
                    else if (blog.AuthorId == request.AuthorId) isAllowed = true;
                    // 4. Admin is always allowed
                    else
                    {
                        var user = await _context.Users.FindAsync(request.AuthorId);
                        if (user?.Role == "admin") isAllowed = true;
                    }

                    if (!isAllowed)
                    {
                        throw new UnauthorizedAccessException("You are not authorized to comment on this private blog.");
                    }
                }
            }
            // Public blogs: Everyone authenticated can comment (handled by Controller [Authorize])

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                BlogId = request.BlogId,
                AuthorId = request.AuthorId,
                Content = request.Content,
                CreatedAt = GetHanoiTime(),
                UpdatedAt = GetHanoiTime()
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Return with author info
            var savedComment = await _context.Comments
                .Include(c => c.Author)
                .FirstAsync(c => c.Id == comment.Id);

            return new CommentResponseDto
            {
                Id = savedComment.Id,
                BlogId = savedComment.BlogId,
                AuthorId = savedComment.AuthorId,
                AuthorFullName = savedComment.Author?.FullName ?? "Unknown",
                Content = savedComment.Content,
                CreatedAt = savedComment.CreatedAt,
                UpdatedAt = savedComment.UpdatedAt,
                Role = savedComment.Author?.Role
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        private static BlogResponseDto MapToResponseDto(Blog blog)
        {
            return new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                AuthorFullName = blog.Author?.FullName ?? "Unknown",
                CourseId = blog.CourseId,
                CourseName = blog.Course?.Name ?? "Unknown",
                ClassId = blog.ClassId,
                IsPrivate = blog.IsPrivate,
                Status = blog.Status,
                Keywords = blog.Keywords,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                Role = blog.Author?.Role
            };
        }
    }
}
