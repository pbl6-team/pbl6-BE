using Application.Contract.Admins.Dtos;

namespace Application.Contract.Admins;
public interface IAdminService
{
    Task<PagedResult<AdminDto>> GetAllAsync(int pageSize, int pageNumber, short status);
    Task<Guid> UpdateAdminStatusAsync(Guid adminId, short status);
    Task<AdminDetailDto> GetByIdAsync(Guid adminId);
    Task<PagedResult<AdminDto>> SearchAsync(string searchValue, int pageSize, int pageNumber, short status);
    Task<Guid> CreateAsync(CreateAdminDto createAdminDto);
    Task<Guid> UpdateAsync(Guid adminId, UpdateAdminDto updateAdminDto);
}