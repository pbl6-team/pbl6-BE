using PBL6.Domain.Models.Admins;

namespace PBL6.Domain.Data.Admins
{
    public interface IAdminRepository : IRepository<AdminAccount>
    {
        Task<AdminAccount> CheckAccountValidAsync(string userName, string password);        
    }
}