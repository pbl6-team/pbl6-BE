using System.Net;

namespace PBL6.Common.Exceptions
{
    public class NotAMemberOfWorkspaceException : CustomException
    {
        public NotAMemberOfWorkspaceException()
            : base("User is not a member of this workspace", (int)HttpStatusCode.BadRequest) { }

    }

    public class SuspendedWorkspaceException : CustomException
    {
        public SuspendedWorkspaceException()
            : base("Workspace is suspended", (int)HttpStatusCode.UnavailableForLegalReasons) { }

    }
}