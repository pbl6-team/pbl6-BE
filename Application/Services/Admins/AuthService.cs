using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Admins;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Common;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Admins;
using workspace.PBL6.Common;

namespace PBL6.Application.Services.Admins
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly string _className;

        public AuthService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _className = GetType().Name;
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<TokenData> SignInAsync(UserLoginDto userLogin)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var existAccount = await _unitOfWork.Admins.CheckAccountValidAsync(userLogin.Username, userLogin.Password);

                ClaimData claimData = new()
                {
                    UserId = existAccount.Id,
                    Username = existAccount.Username,
                    Email = existAccount.Email,
                    IsActive = existAccount.IsActive,
                    IsAdmin = true
                };
                
                var token = SecurityFunction.GenerateToken(claimData, _config);
                var refreshToken = SecurityFunction.GenerateRandomString();
                var adminToken = await _unitOfWork.AdminTokens.AddAsync(new AdminToken
                {
                    AdminId = existAccount.Id,
                    Token = token,
                    TimeOut = DateTime.Now.AddMinutes( int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut)),
                    RefreshToken = refreshToken,
                    RefreshTokenTimeOut = DateTime.Now.AddMinutes( int.Parse(_config["RefreshTokenTimeOut"] ?? CommonConfig.OtpTimeOut)),
                });

                await _unitOfWork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return new TokenData
                {
                    UserId = existAccount.Id,
                    Email = existAccount.Email,
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenTimeOut = adminToken.TimeOut,
                    RefreshTokenTimeOut = adminToken.RefreshTokenTimeOut.Value,
                    TokenType = JwtBearerDefaults.AuthenticationScheme
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{_className}][{method}] Error", _className, method);
                throw;
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordDto changePassword)
        {
            var method = GetActualAsyncMethodName();
            var now = DateTimeOffset.Now;
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedException("User is not exist or inactive"));
                var currentUser = await _unitOfWork.Admins.FindAsync(userId) ?? throw new UnauthorizedException("User not found");
                await _unitOfWork.Admins.CheckAccountValidAsync(currentUser.Username, changePassword.CurrentPassword);

                var validToken = await _unitOfWork.AdminTokens.Queryable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.AdminId == userId
                            && x.Otp == changePassword.Otp
                            && x.OtpType == ((short)OTP_TYPE.CHANGE_PASSWORD)
                            && x.ValidTo >= now
                            && x.TimeOut >= now
                    );
                if (validToken == null)
                {
                    throw new BadRequestException("OTP is invalid or expired");
                }
                else
                {
                    validToken.ValidTo = now;
                    validToken.TimeOut = now;
                    await _unitOfWork.AdminTokens.UpdateAsync(validToken);
                }

                var passwordSalt = SecurityFunction.GenerateRandomString();
                var passwordHash = SecurityFunction.HashPassword(changePassword.NewPassword, passwordSalt);
                currentUser.PasswordSalt = passwordSalt;
                currentUser.Password = passwordHash;
                await _unitOfWork.Admins.UpdateAsync(currentUser);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{_className}][{method}] Error", _className, method);
                throw;
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordRequest)
        {
            var method = GetActualAsyncMethodName();
            var now = DateTimeOffset.UtcNow;
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var currentUser =
                    await _unitOfWork.Admins
                        .Queryable()
                        .FirstOrDefaultAsync(
                            x => x.Email == forgotPasswordRequest.Email && x.IsActive
                        )
                    ?? throw new BadRequestException("Email is not exist or user is inactive");
                var validToken = await _unitOfWork.AdminTokens
                    .Queryable()
                    .Include(x => x.AdminAccount)
                    .FirstOrDefaultAsync(
                        x =>
                            x.AdminAccount.Email == forgotPasswordRequest.Email
                            && x.Otp == forgotPasswordRequest.Otp
                            && x.OtpType == ((short)OTP_TYPE.FORGOT_PASSWORD)
                            && x.ValidTo >= now
                            && x.TimeOut >= now
                    );
                if (validToken is null)
                {
                    throw new BadRequestException("OTP is invalid or expired");
                }
                else
                {
                    validToken.ValidTo = now;
                    validToken.TimeOut = now;
                    await _unitOfWork.AdminTokens.UpdateAsync(validToken);
                }

                var passwordSalt = SecurityFunction.GenerateRandomString();
                var hashPassword = SecurityFunction.HashPassword(
                    forgotPasswordRequest.NewPassword,
                    passwordSalt
                );
                currentUser.Password = hashPassword;
                currentUser.PasswordSalt = passwordSalt;
                await _unitOfWork.Admins.UpdateAsync(currentUser);
                await _unitOfWork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);
            }
            catch (Exception e)
            {
                _logger.LogInformation(
                    "[{_className}][{method}] Error: {message}",
                    _className,
                    method,
                    e.Message
                );

                throw;
            }
        }

        public async Task GetNewOtpAsync(GetOtpDto getOtpDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userQueryable = _unitOfWork.Admins.Queryable();
                var existedAdmin =
                    await userQueryable
                        .Include(x => x.AdminTokens)
                        .FirstOrDefaultAsync(x => x.Email == getOtpDto.Email)
                    ?? throw new BadRequestException(
                        $"Not exist user with email '{getOtpDto.Email}'"
                    );

                if (getOtpDto.OtpType == ((short)OTP_TYPE.VERIFY_USER) && existedAdmin.IsActive)
                    throw new BadRequestException("Email is verified");

                var existToken = await _unitOfWork.AdminTokens
                    .Queryable()
                    .OrderByDescending(x => x.ValidTo)
                    .FirstOrDefaultAsync(
                        x =>
                            x.Otp != null
                            && x.AdminId == existedAdmin.Id
                            && x.ValidTo >= DateTimeOffset.UtcNow
                            && x.TimeOut >= DateTimeOffset.UtcNow
                    );
                if (existToken is not null)
                    throw new ExistedOtpException(
                        (int)(existToken.ValidTo - DateTimeOffset.UtcNow).TotalSeconds
                    );
                var OTP = SecurityFunction.GenerationOTP();
                ClaimData claimData =
                    new()
                    {
                        UserId = existedAdmin.Id,
                        Email = existedAdmin.Email,
                        Username = existedAdmin.Username,
                        IsActive = existedAdmin.IsActive,
                    };

                var token = SecurityFunction.GenerateToken(claimData, _config);
                await _unitOfWork.AdminTokens.AddAsync(
                    new()
                    {
                        AdminId = existedAdmin.Id,
                        Token = token,
                        Otp = OTP,
                        OtpType = getOtpDto.OtpType,
                        ValidTo = DateTimeOffset.UtcNow.AddMinutes(
                            int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut)
                        ),
                        TimeOut = DateTimeOffset.UtcNow.AddMinutes(
                            int.Parse(_config["TokenTimeOut"] ?? CommonConfig.OtpTimeOut)
                        )
                    }
                );
                await _unitOfWork.SaveChangeAsync();
                var subject = MailConsts.SignUp.Subject;
                var template = MailConsts.SignUp.Template;
                switch (getOtpDto.OtpType)
                {
                    case (short)OTP_TYPE.CHANGE_PASSWORD:
                        subject = MailConsts.ChangePassword.Subject;
                        template = MailConsts.ChangePassword.Template;
                        break;
                    case ((short)OTP_TYPE.FORGOT_PASSWORD):
                        subject = MailConsts.ForgotPassword.Subject;
                        template = MailConsts.ForgotPassword.Template;
                        break;
                }

                await _mailService.Send(existedAdmin.Email, subject, template, OTP);

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{_className}][{method}] Error", _className, method);
                throw;
            }
        }

        public async Task<TokenData> RefreshTokenAsync(string refreshToken)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var now = DateTimeOffset.UtcNow;
                var userToken =
                    _unitOfWork.AdminTokens
                        .Queryable()
                        .Include(x => x.AdminAccount)
                        .FirstOrDefault(
                            x =>
                                x.RefreshToken == refreshToken
                                && x.RefreshTokenTimeOut >= now
                                && x.TimeOut < now
                        ) ?? throw new BadRequestException("Invalid refresh token");
                var claimData = new ClaimData
                {
                    UserId = userToken.AdminId,
                    Email = userToken.AdminAccount.Email,
                    Username = userToken.AdminAccount.Username,
                    IsActive = userToken.AdminAccount.IsActive
                };
                var token = SecurityFunction.GenerateToken(claimData, _config);
                userToken.Token = token;
                userToken.TimeOut = DateTimeOffset.UtcNow.AddMinutes(
                    int.Parse(_config["TokenTimeOut"] ?? CommonConfig.OtpTimeOut)
                );
                await _unitOfWork.AdminTokens.UpdateAsync(userToken);
                await _unitOfWork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return new TokenData
                {
                    UserId = userToken.AdminId,
                    Email = userToken.AdminAccount.Email,
                    Token = token,
                    TokenTimeOut = userToken.TimeOut,
                    RefreshToken = refreshToken,
                    RefreshTokenTimeOut = (DateTimeOffset)userToken.RefreshTokenTimeOut,
                    TokenType = JwtBearerDefaults.AuthenticationScheme
                };
            }
            catch (Exception e)
            {
                _logger.LogInformation(
                    "[{_className}][{method}] Error: {message}",
                    _className,
                    method,
                    e.Message
                );
                throw;
            }
        }
    }
}
