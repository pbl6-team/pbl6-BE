using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users 
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> CheckIsUserAsync(Guid userId);
        Task<User> GetUserByIdAsync(Guid userId);
        Task<User> GetUserByEmailAsync(string email);
    }
}