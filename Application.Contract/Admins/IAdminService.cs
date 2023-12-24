using Application.Contract.Admins.Dtos;

namespace Application.Contract.Admins;
public interface IAdminService
{
    Task<IEnumerable<AdminDto>> GetAllAsync();
    Task<Guid> UpdateAdminStatusAsync(Guid adminId, short status);
    Task<AdminDetailDto> GetByIdAsync(Guid adminId);
    Task<IEnumerable<AdminDto>> SearchAsync(short searchType, string searchValue, int numberOfResults);
    Task<Guid> CreateAsync(CreateAdminDto createAdminDto);
    Task<Guid> UpdateAsync(Guid adminId, UpdateAdminDto updateAdminDto);
}