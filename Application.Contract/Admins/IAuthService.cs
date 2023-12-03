using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.Application.Contract.Admins
{
    public interface IAuthService
    {
        Task<TokenData> SignInAsync(UserLoginDto userLogin);
        Task GetNewOtpAsync(GetOtpDto getOtpDto);
        Task ChangePasswordAsync(ChangePasswordDto changePassword);
        Task ForgotPasswordAsync(ForgotPasswordDto forgotPassword);
        Task<TokenData> RefreshTokenAsync(string refreshToken);
    }
}
