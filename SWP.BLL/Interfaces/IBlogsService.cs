using SWP.BLL.DTOs.Blogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP.BLL.Interfaces
{
    public interface IBlogsService
    {
        Task<IEnumerable<BlogResponseDto>> GetAllBlogsForAdminAsync(Guid? courseId = null);
        Task<IEnumerable<BlogResponseDto>> GetPrivateBlogsAsync(Guid? courseId = null);
        Task<IEnumerable<BlogResponseDto>> GetAllPublicBlogsAsync(Guid? courseId = null, int? status = 1);
        Task<IEnumerable<BlogResponseDto>> GetClassBlogsAsync(string classId, Guid? courseId = null, int? status = 1);
        Task<IEnumerable<BlogResponseDto>> GetAllBlogsAsync();
        Task<BlogResponseDto> GetBlogByIdAsync(Guid id);
        Task<BlogResponseDto> CreateBlogAsync(BlogRequestDto request);
        Task<BlogResponseDto> UpdateBlogAsync(Guid id, BlogRequestDto request);
        Task<bool> ApproveBlogAsync(Guid id, int status);
        Task<bool> DeleteBlogAsync(Guid id);

        // Comments
        Task<IEnumerable<CommentResponseDto>> GetCommentsByBlogIdAsync(Guid blogId);
        Task<CommentResponseDto> CreateCommentAsync(CommentRequestDto request);
        Task<bool> DeleteCommentAsync(Guid commentId);
    }
}
