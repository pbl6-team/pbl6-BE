using System.Net;

namespace PBL6.Common.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException(string message, int statusCode = (int)HttpStatusCode.BadRequest) : base(message)
        { 
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }
    }
}