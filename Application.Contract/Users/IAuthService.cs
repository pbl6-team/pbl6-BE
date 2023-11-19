using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.Application.Contract.Users
{
    public interface IAuthService
    {
        Task<UserRegisterResponse> RegisterAsync(UserRegisterDto userRegister);
        Task<TokenData> VerifyRegisterAsync(VerifyRegisterDto verifyRegister);
        Task<TokenData> SigninAsync(UserLoginDto userLogin);
        Task GetNewOtpAsync(GetOtpDto getOtpDto);
        Task ChangePasswordAsync(ChangePasswordDto changePassword);
        Task ForgotPasswordAsync(ForgotPasswordDto forgotPassword);
        Task<TokenData> GoogleLoginAsync(string oauthCode);
        Task<TokenData> RefreshTokenAsync(string refreshToken);
    }
}
