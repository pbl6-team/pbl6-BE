using PBL6.Common.Exceptions;

namespace PBL6.Common
{
    public class EmailExistedException : CustomException
    {
        public EmailExistedException(string email)
            : base($"Already have an account using this email '{email}'") { }
    }

    public class UsernameExistedException : CustomException
    {
        public UsernameExistedException(string username)
            : base($"Already have an account using this username '{username}'") { }
    }

    public class InvalidOtpException : CustomException
    {
        public InvalidOtpException()
            : base($"OTP is invalid or expired") { }
    }

    public class ExistedOtpException : CustomException
    {
        public ExistedOtpException(int second)
            : base($"Please try after {second} seconds!") { }
    }

    public class InvalidUsernamePasswordException : CustomException
    {
        public InvalidUsernamePasswordException()
            : base($"Username or password is invalid") { }
    }

    public class InvalidExternalAuthenticationException : CustomException
    {
        public InvalidExternalAuthenticationException()
            : base($"Invalid External Authentication") { }
    }

    public class BlockedUserException : CustomException
    {
        public BlockedUserException()
            : base($"Your account has been blocked") { }
    }
}