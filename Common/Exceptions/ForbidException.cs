using System.Net;

namespace PBL6.Common.Exceptions
{
    public class ForbidException : CustomException
    {
        public ForbidException()
            : base("You are not authorized to perform this action", ((int)HttpStatusCode.Forbidden)) { }                
    }
}