using System.Net;

namespace PBL6.Common.Exceptions
{
    public class BadRequestException : CustomException
    {
        public BadRequestException(string message, int statusCode = (int)HttpStatusCode.BadRequest) : base(message, statusCode)
        {
        }
    }
}