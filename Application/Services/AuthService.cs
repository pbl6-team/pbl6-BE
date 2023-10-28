using System.Runtime.CompilerServices;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Common;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Data;
using PBL6.Domain.Models.Users;
using workspace.PBL6.Common;

namespace PBL6.Application.Services
{
    public class AuthService : BaseService,  IAuthService
    {
        private readonly IUnitOfwork _unitOfwork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly string _className;

        public AuthService(
          IUnitOfwork unitOfwork,
          IMapper mapper,
          ILogger<AuthService> logger,
          IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            _unitOfwork = unitOfwork;
            _logger = logger;
            _mapper = mapper;
            _className = GetType().Name;
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<UserRegisterResponse> RegisterAsync(UserRegisterDto userInput)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userQueryable = _unitOfwork.Users.Queryable();
                if (userQueryable.Any(x => x.Email == userInput.Email && x.IsActive))
                    throw new EmailExistedException(userInput.Email);

                if (userQueryable.Any(x => x.Username == userInput.Username && x.IsActive))
                    throw new UsernameExistedException(userInput.Username);

                var passwordSalt = SecurityFunction.GenerateRandomString();
                var hashPassword = SecurityFunction.HashPassword(userInput.Password, passwordSalt);
                var OTP = SecurityFunction.GenerationOTP();

                var existedUser = await userQueryable
                    .Include(x => x.UserTokens)
                    .FirstOrDefaultAsync(x => x.Email == userInput.Email || x.Username == userInput.Username);
                if (existedUser is null)
                {
                    User newUser = new()
                    {
                        Email = userInput.Email,
                        Username = userInput.Username,
                        Password = hashPassword,
                        PasswordSalt = passwordSalt,
                        IsActive = false,
                        Information = new()
                        {
                            FirstName = userInput.FirstName,
                            LastName = userInput.LastName,
                            BirthDay = userInput.BirthDay,
                            Phone = userInput.Phone,
                            Gender = userInput.Gender,
                            Status = (short)USER.DEFAULT
                        }
                    };

                    existedUser = await _unitOfwork.Users.AddAsync(newUser);
                }
                else
                {
                    existedUser.Username = userInput.Username;
                    existedUser.Password = hashPassword;
                    existedUser.PasswordSalt = passwordSalt;
                    existedUser.IsActive = false;
                    existedUser.Information = new()
                    {
                        FirstName = userInput.FirstName,
                        LastName = userInput.LastName,
                        BirthDay = userInput.BirthDay,
                        Phone = userInput.Phone,
                        Gender = userInput.Gender,
                        Status = (short)USER.DEFAULT
                    };

                    await _unitOfwork.Users.UpdateAsync(existedUser);
                }

                ClaimData claimData = new()
                {
                    UserId = existedUser.Id,
                    Email = existedUser.Email,
                    Username = existedUser.Username,
                    IsActive = existedUser.IsActive,
                };
                var token = SecurityFunction.GenerateToken(claimData, _config);
                await _unitOfwork.UserTokens.AddAsync(
                    new()
                    {
                        UserId = existedUser.Id,
                        Token = token,
                        Otp = OTP,
                        OtpType = (short?)OTP_TYPE.VERIFY_USER,
                        ValidTo = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut)),
                        TimeOut = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut))
                    }
                );

                await _unitOfwork.SaveChangeAsync();
                await _mailService.Send(userInput.Email, MailConsts.SignUp.Subject, MailConsts.SignUp.Template, OTP);

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return new UserRegisterResponse { Token = token, Email = userInput.Email };
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task VerifyRegisterAsync(VerifyRegisterDto verifyRegisterDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(_currentUser.UserId);
                var userToken = await _unitOfwork.UserTokens
                    .Queryable()
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                        x.User.Id == userId
                        && !x.User.IsActive
                        && !x.User.IsDeleted
                        && x.Otp == verifyRegisterDto.Otp
                        && x.ValidTo >= DateTimeOffset.UtcNow
                        && x.TimeOut >= DateTimeOffset.UtcNow
                    ) ?? throw new InvalidOtpException();
                userToken.ValidTo = DateTimeOffset.UtcNow;
                userToken.User.IsActive = true;
                await _unitOfwork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task GetNewOtpAsync(GetOtpDto getOtpDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userQueryable = _unitOfwork.Users.Queryable();
                var existedUser = await userQueryable
                    .Include(x => x.UserTokens)
                    .FirstOrDefaultAsync(x => x.Email == getOtpDto.Email);

                if (existedUser is null)
                {
                    return;
                }

                var existToken = await _unitOfwork.UserTokens
                    .Queryable()
                    .FirstOrDefaultAsync(x => x.Otp != null
                        && x.ValidTo >= DateTimeOffset.UtcNow
                        && x.TimeOut >= DateTimeOffset.UtcNow
                    );
                if (existToken is not null) throw new ExistedOtpException(existToken.ValidTo - DateTimeOffset.UtcNow);
                var OTP = SecurityFunction.GenerationOTP();
                ClaimData claimData = new()
                {
                    UserId = existedUser.Id,
                    Email = existedUser.Email,
                    Username = existedUser.Username,
                    IsActive = existedUser.IsActive,
                };

                var token = SecurityFunction.GenerateToken(claimData, _config);
                await _unitOfwork.UserTokens.AddAsync(
                    new()
                    {
                        UserId = existedUser.Id,
                        Token = token,
                        Otp = OTP,
                        OtpType = getOtpDto.OtpType,
                        ValidTo = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut)),
                        TimeOut = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["TokenTimeOut"] ?? CommonConfig.OtpTimeOut))
                    }
                );
                await _unitOfwork.SaveChangeAsync();
                await _mailService.Send(existedUser.Email, MailConsts.SignUp.Subject, MailConsts.SignUp.Template, OTP);

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<TokenData> SigninAsync(UserLoginDto userLogin)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var existedUser = await _unitOfwork.Users
                    .Queryable().Include(x => x.UserTokens)
                    .FirstOrDefaultAsync(x => (x.Username == userLogin.Username || x.Email == userLogin.Username) && x.IsActive) ?? throw new InvalidUsernamePasswordException();
                var passwordSalt = existedUser.PasswordSalt;
                var hashPassword = SecurityFunction.HashPassword(userLogin.Password, passwordSalt);
                if (existedUser.Password != hashPassword)
                {
                    throw new InvalidUsernamePasswordException();
                }

                ClaimData claimData = new()
                {
                    UserId = existedUser.Id,
                    Email = existedUser.Email,
                    Username = existedUser.Username,
                    IsActive = existedUser.IsActive,
                };
                var refresshToken = SecurityFunction.GenerateRandomString();
                var token = SecurityFunction.GenerateToken(claimData, _config);
                await _unitOfwork.UserTokens.AddAsync(
                    new()
                    {
                        UserId = existedUser.Id,
                        Token = token,
                        Otp = null,
                        ValidTo = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut)),
                        TimeOut = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["TokenTimeOut"] ?? CommonConfig.OtpTimeOut)),
                    RefreshTokenTimeOut = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["RefreshTokenTimeOut"] ?? CommonConfig.OtpTimeOut)),
                        RefreshToken = refresshToken
                    }
                );

                await _unitOfwork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return new TokenData { UserId = existedUser.Id, Email = existedUser.Email, Token = token, RefreshToken = refresshToken, TokenType = JwtBearerDefaults.AuthenticationScheme };
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<TokenData> GoogleLoginAsync(string oauthCode)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var payload = await SecurityFunction.VerifyGoogleToken(oauthCode, _config) ?? throw new BadRequestException("Invalid External Authentication.");
                GoogleOauthUser googleOauthUser = new()
                {
                    Name = payload.Name,
                    Email = payload.Email,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
                var existedUser = await _unitOfwork.Users
                   .Queryable().Include(x => x.UserTokens)
                   .FirstOrDefaultAsync(x => x.Email == googleOauthUser.Email);

                if (existedUser is null)
                {
                    User newUser = new()
                    {
                        Email = googleOauthUser.Email,
                        Username = googleOauthUser.Email,
                        Information = new()
                        {
                            FirstName = googleOauthUser.Name,
                            Status = (short)USER.DEFAULT
                        }                       
                    };

                    existedUser = await _unitOfwork.Users.AddAsync(newUser);
                }
                else
                {
                    existedUser.IsActive = true;
                    await _unitOfwork.Users.UpdateAsync(existedUser);
                }

                ClaimData claimData = new()
                {
                    UserId = existedUser.Id,
                    Email = existedUser.Email,
                    Username = existedUser.Username,
                    IsActive = existedUser.IsActive,
                };
                var refresshToken = SecurityFunction.GenerateRandomString();
                var token = SecurityFunction.GenerateToken(claimData, _config);
                await _unitOfwork.UserTokens.AddAsync(
                    new()
                    {
                        UserId = existedUser.Id,
                        Token = token,
                        Otp = null,
                        ValidTo = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["OtpTimeOut"] ?? CommonConfig.OtpTimeOut)),
                        TimeOut = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["TokenTimeOut"] ?? CommonConfig.OtpTimeOut)),
                    RefreshTokenTimeOut = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["RefreshTokenTimeOut"] ?? CommonConfig.OtpTimeOut)),
                        RefreshToken = refresshToken
                    }
                );

                await _unitOfwork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return new TokenData { UserId = existedUser.Id, Email = existedUser.Email, Token = token, RefreshToken = refresshToken, TokenType = JwtBearerDefaults.AuthenticationScheme };
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var method = GetActualAsyncMethodName();
            var now = DateTimeOffset.UtcNow;
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(_currentUser.UserId);
                var currentUser = await _unitOfwork.Users.Queryable().FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
                if (currentUser is null)
                {
                    throw new BadRequestException("User is not exist or inactive");
                }

                var validToken = await _unitOfwork.UserTokens.Queryable()
                    .FirstOrDefaultAsync(x => x.Otp == changePasswordDto.OTP 
                        && x.OtpType == ((short)OTP_TYPE.CHANGE_PASSWORD)
                        && x.ValidTo >= now
                        && x.TimeOut >= now
                    );
                if (validToken is null)
                {
                    throw new BadRequestException("OTP is invalid or expried");
                }
                else
                {
                    validToken.ValidTo = now;
                    validToken.TimeOut = now;
                    await _unitOfwork.UserTokens.UpdateAsync(validToken);
                }

                var hashInputPassword = SecurityFunction.HashPassword(changePasswordDto.CurrentPassword, currentUser.PasswordSalt);
                if (currentUser.Password != hashInputPassword)
                {
                    throw new BadRequestException("Invalid password");
                }

                var passwordSalt = SecurityFunction.GenerateRandomString();
                var hashPassword = SecurityFunction.HashPassword(changePasswordDto.NewPassword, passwordSalt);
                currentUser.Password = hashPassword;
                currentUser.PasswordSalt = passwordSalt;
                await _unitOfwork.Users.UpdateAsync(currentUser);
                await _unitOfwork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public Task ForgotPasswordAsync()
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }
    }
}