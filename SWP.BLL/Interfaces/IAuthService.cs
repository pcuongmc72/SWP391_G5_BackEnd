using System.Threading.Tasks;

namespace SWP.BLL.Interfaces
{
    public interface IAuthService
    {
        /// <summary>Đăng nhập, trả về JWT token nếu thành công.</summary>
        Task<string?> LoginAsync(string email, string password);
    }
}
