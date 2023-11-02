using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.Application.Contract.Users
{
    public interface IAuthService
    {
       Task<UserRegisterResponse> RegisterAsync(UserRegisterDto userRegister);
       Task VerifyRegisterAsync(VerifyRegisterDto verifyRegister);
       Task<TokenData> SigninAsync(UserLoginDto userLogin);
       Task GetNewOtpAsync(GetOtpDto getOtpDto);
       Task ChangePasswordAsync(ChangePasswordDto changePassword);
       Task ForgotPasswordAsync();
       Task<TokenData> GoogleLoginAsync(string oauthCode);
    }
}