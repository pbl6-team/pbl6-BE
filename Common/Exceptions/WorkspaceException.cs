using System.Net;

namespace PBL6.Common.Exceptions
{
    public class NotAMemberOfWorkspaceException : CustomException
    {
        public NotAMemberOfWorkspaceException()
            : base("User is not a member of this workspace", (int)HttpStatusCode.BadRequest) { }
    }
}