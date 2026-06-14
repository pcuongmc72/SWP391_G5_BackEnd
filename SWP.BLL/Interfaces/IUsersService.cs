using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.DAL.Models;

namespace SWP.BLL.Interfaces
{
    public interface IUsersService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
    }
}
