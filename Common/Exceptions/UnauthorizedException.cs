using System.Net;

namespace PBL6.Common.Exceptions
{
    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message) : base(message, (int)HttpStatusCode.Unauthorized)
        {
        }        
    }
}