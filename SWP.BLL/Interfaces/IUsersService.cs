using SWP.BLL.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.Interfaces
{
    public interface IUsersService
    {
        Task<UserResponseDto> CreateUserAsync(RegisterRequestDto request);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? role, string? searchTerm);
        Task<UserResponseDto> GetUserByIdAsync(string id);
        Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto request);
    }
}
