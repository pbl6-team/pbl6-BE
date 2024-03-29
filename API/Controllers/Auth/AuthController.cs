using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.API.Filters;
using PBL6.Api.Controllers;

namespace PBL6.API.Controllers.Workspaces
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// API để đăng kí user mới
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Đăng ký thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserRegisterResponse))]
        public async Task<IActionResult> Signup(UserRegisterDto input)
        {
            return Ok(await _authService.RegisterAsync(input));
        }

        /// <summary>
        /// Api xác thực otp sau khi đăng kí
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Xác thực email thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("verify-register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        [AuthorizeFilter]
        public async Task<IActionResult> VerifyRegister(VerifyRegisterDto input)
        {
            return Ok(await _authService.VerifyRegisterAsync(input));
        }

        /// <summary>
        /// API đăng nhập bằng tài khoản và mật khẩu
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Đăng ký thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("signin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        public async Task<IActionResult> Login(UserLoginDto input)
        {
            return Ok(await _authService.SignInAsync(input));
        }

        /// <summary>
        /// API đăng nhập/ đăng ký bằng tài khoản google
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <response code="200">Đăng ký/Đăng nhập thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("signin-google")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        public async Task<IActionResult> ExternalLogin(string code)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            return Ok(await _authService.GoogleLoginAsync(code));
        }

        /// <summary>
        /// API đổi mật khẩu
        /// </summary>
        /// <param name="changePasswordDto"></param>
        /// <returns></returns>
        /// <response code="200">Đổi mật khẩu thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AuthorizeFilter]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            await _authService.ChangePasswordAsync(changePasswordDto);

            return Ok();
        }

        /// <summary>
        /// API Get OTP
        /// OTP_TYPE { VERIFY_USER = 1,
        /// CHANGE_PASSWORD=2,
        /// FORGOT_PASSWORD=3}
        /// </summary>
        /// <param name="getOtpDto"></param>
        /// <returns></returns>
        /// <response code="200">Lấy OTP thành công và đã gửi mail cho user</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("get-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOtp(GetOtpDto getOtpDto)
        {
            await _authService.GetNewOtpAsync(getOtpDto);

            return Ok();
        }

        /// <summary>
        /// API Get forgot password
        /// </summary>
        /// <param name="forgotPasswordRequest"></param>
        /// <returns></returns>
        /// <response code="200">Đổi mật khẩu thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordRequest)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            return Ok();
        }

        /// <summary>
        /// API get token use refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        /// <response code="200">Lấy token thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            return Ok(await _authService.RefreshTokenAsync(refreshToken));
        }
    }
}
