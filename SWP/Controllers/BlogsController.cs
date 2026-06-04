using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Blogs;
using SWP.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SWP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly IBlogsService _blogsService;

        public BlogsController(IBlogsService blogsService)
        {
            _blogsService = blogsService;
        }

        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetAllPublicBlogs([FromQuery] Guid? courseId)
        {
            var blogs = await _blogsService.GetAllPublicBlogsAsync(courseId);
            return Ok(blogs);
        }

        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetAllBlogs([FromQuery] Guid? courseId)
        {
            var blogs = await _blogsService.GetAllBlogsForAdminAsync(courseId);
            return Ok(blogs);
        }

        [HttpGet("private")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetPrivateBlogs([FromQuery] Guid? courseId)
        {
            var blogs = await _blogsService.GetPrivateBlogsAsync(courseId);
            return Ok(blogs);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "admin,lecturer")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetPendingBlogs()
        {
            var blogs = await _blogsService.GetAllPublicBlogsAsync(null, 0);
            return Ok(blogs);
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetClassBlogs(string classId, [FromQuery] Guid? courseId)
        {
            var blogs = await _blogsService.GetClassBlogsAsync(classId, courseId);
            return Ok(blogs);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetUserBlogs(string userId)
        {
            var blogs = await _blogsService.GetUserBlogsAsync(userId);
            return Ok(blogs);
        }

        [HttpGet("my-classes/{studentId}")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetStudentClassBlogs(string studentId, [FromQuery] Guid? courseId)
        {
            var blogs = await _blogsService.GetStudentClassBlogsAsync(studentId, courseId);
            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlogResponseDto>> GetBlogById(Guid id)
        {
            var blog = await _blogsService.GetBlogByIdAsync(id);
            if (blog == null) return NotFound();
            return Ok(blog);
        }

        [HttpPost]
        public async Task<ActionResult<BlogResponseDto>> CreateBlog([FromBody] BlogRequestDto request)
        {
            try
            {
                var blog = await _blogsService.CreateBlogAsync(request);
                return CreatedAtAction(nameof(GetBlogById), new { id = blog.Id }, blog);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BlogResponseDto>> UpdateBlog(Guid id, [FromBody] BlogRequestDto request)
        {
            var updatedBlog = await _blogsService.UpdateBlogAsync(id, request);
            if (updatedBlog == null) return NotFound();
            return Ok(updatedBlog);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "admin,lecturer")]
        public async Task<ActionResult> ApproveBlog(Guid id, [FromQuery] int status)
        {
            var result = await _blogsService.ApproveBlogAsync(id, status);
            if (!result) return NotFound();
            return Ok(new { message = "Status updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(Guid id)
        {
            var result = await _blogsService.DeleteBlogAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // --- Comments Endpoints ---

        [HttpGet("{blogId}/comments")]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetComments(Guid blogId)
        {
            var comments = await _blogsService.GetCommentsByBlogIdAsync(blogId);
            return Ok(comments);
        }

        [HttpPost("comments")]
        [Authorize(Roles = "admin,lecturer,student")]
        public async Task<ActionResult<CommentResponseDto>> CreateComment([FromBody] CommentRequestDto request)
        {
            try
            {
                var comment = await _blogsService.CreateCommentAsync(request);
                return Ok(comment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("comments/{id}")]
        [Authorize(Roles = "admin,lecturer,student")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var result = await _blogsService.DeleteCommentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
