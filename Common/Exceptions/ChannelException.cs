using System.Net;

namespace PBL6.Common.Exceptions
{
    public class NotAMemberOfChannelException : CustomException
    {
        public NotAMemberOfChannelException()
            : base("User is not a member of this channel", (int)HttpStatusCode.BadRequest) { }
    }
}