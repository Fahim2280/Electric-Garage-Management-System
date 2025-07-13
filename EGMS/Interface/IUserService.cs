using EGMS.DTOs;

namespace EGMS.Interface
{
    public interface IUserService
    {
        Task<bool> Register(UserRegisterDTO dto);
        Task<string> Login(UserLoginDTO dto);
    }

}
