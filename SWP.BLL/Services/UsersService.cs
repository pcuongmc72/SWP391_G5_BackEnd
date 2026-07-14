using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Users;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.Services
{
    public class UsersService : IUsersService
    {
        private readonly FlippedClassroomContext _context;

        public UsersService(FlippedClassroomContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Id == request.Id))
                throw new InvalidOperationException("Mã định danh (Id) này đã tồn tại trên hệ thống.");

            var newUser = new User
            {
                Id = request.Id,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role.ToLower(),
                AvatarUrl = request.AvatarUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return MapToDto(newUser);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? role, string? searchTerm)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role.ToLower());

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(u => u.FullName.Contains(searchTerm)
                                      || u.Email.Contains(searchTerm)
                                      || u.Id.Contains(searchTerm));

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy người dùng.");

            return MapToDto(user);
        }

        public async Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy người dùng.");

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.AvatarUrl = request.AvatarUrl;
            user.IsActive = request.IsActive;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        

        // Hàm phụ trợ map từ Entity sang DTO
        private static UserResponseDto MapToDto(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
