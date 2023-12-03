using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Admins;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Filters;

namespace PBL6.API.Controllers.Admins.Auth
{
    [Produces("application/json")]
    [Route("api/admin/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Sign in to the system
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [HttpPost("SignIn")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        public async Task<IActionResult> SignInAsync(UserLoginDto userLogin)
        {
            var result = await _authService.SignInAsync(userLogin);
            return Ok(result);
        }

        /// <summary>
        /// Get new OTP
        /// </summary>
        /// <param name="getOtpDto"></param>
        /// <returns></returns>
        [HttpPost("GetNewOtp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNewOtpAsync(GetOtpDto getOtpDto)
        {
            await _authService.GetNewOtpAsync(getOtpDto);
            return Ok();
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="changePassword"></param>
        /// <returns></returns>
        [HttpPost("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        [AdminFilter]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePassword)
        {
            await _authService.ChangePasswordAsync(changePassword);
            return Ok();
        }

        /// <summary>
        /// Forgot password
        /// </summary>
        /// <param name="forgotPassword"></param>
        /// <returns></returns>
        [HttpPost("ForgotPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordDto forgotPassword)
        {
            await _authService.ForgotPasswordAsync(forgotPassword);
            return Ok();
        }

        /// <summary>
        /// Refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [HttpPost("RefreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenData))]
        public async Task<IActionResult> RefreshTokenAsync(string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }
    }
}
