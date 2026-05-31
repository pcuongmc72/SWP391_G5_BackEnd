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
                .AsQueryable();

            // If we are looking for public blogs (default), filter out private ones.
            // If we are looking for pending blogs (status 0), we want to see both public and private.
            if (status == 1)
            {
                query = query.Where(b => !b.IsPrivate);
            }

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

        public async Task<IEnumerable<BlogResponseDto>> GetAllBlogsAsync()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Course)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

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

                if (!isAllowed)
                {
                    throw new UnauthorizedAccessException("You are not authorized to post a private blog for this class.");
                }
            }

            var blog = new Blog
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId,
                CourseId = request.CourseId,
                ClassId = request.ClassId,
                IsPrivate = request.IsPrivate,
                Keywords = request.Keywords,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetBlogByIdAsync(id);
        }

        public async Task<bool> ApproveBlogAsync(Guid id, int status)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return false;

            blog.Status = status;
            blog.UpdatedAt = DateTime.UtcNow;

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
                UpdatedAt = c.UpdatedAt
            });
        }

        public async Task<CommentResponseDto> CreateCommentAsync(CommentRequestDto request)
        {
            var blog = await _context.Blogs
                .Include(b => b.Class)
                .FirstOrDefaultAsync(b => b.Id == request.BlogId);

            if (blog == null) throw new KeyNotFoundException("Blog not found.");

            // Validation for private blogs
            if (blog.IsPrivate && !string.IsNullOrEmpty(blog.ClassId))
            {
                bool isAllowed = false;

                // 1. Is the commenter the lecturer of the class?
                if (blog.Class?.LecturerId == request.AuthorId)
                {
                    isAllowed = true;
                }
                // 2. Is the commenter a student in the class?
                else if (await _context.ClassStudents.AnyAsync(cs => cs.ClassId == blog.ClassId && cs.StudentId == request.AuthorId))
                {
                    isAllowed = true;
                }
                // 3. Is the commenter the author of the blog post?
                else if (blog.AuthorId == request.AuthorId)
                {
                    isAllowed = true;
                }

                if (!isAllowed)
                {
                    throw new UnauthorizedAccessException("You are not authorized to comment on this private blog.");
                }
            }

            var comment = new Comment
            {
                BlogId = request.BlogId,
                AuthorId = request.AuthorId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
                UpdatedAt = savedComment.UpdatedAt
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
                UpdatedAt = blog.UpdatedAt
            };
        }
    }
}
