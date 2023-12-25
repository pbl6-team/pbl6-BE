namespace PBL6.Common.Consts
{
    public static class MailConst
    {
        public static class SignUp
        {
            public const string Subject = "[FIRA] Welcome to Fira";
            public const string Template = "Signup.cshtml";
        }

        public static class ChangePassword
        {
            public const string Subject = "[FIRA] Change password";
            public const string Template = "ChangePassword.cshtml";
        }

        public static class ForgotPassword
        {
            public const string Subject = "[FIRA] Forgot password";
            public const string Template = "ForgotPassword.cshtml";
        }

        public static class InviteToWorkspace
        {
            public const string Subject = "[FIRA] Invite to workspace";
            public const string Template = "InviteToWorkspace.cshtml";
        }

        public static class AdminRandomPassword
        {
            public const string Subject = "[FIRA] Your admin password";
            public const string Template = "AdminRandomPassword.cshtml";

        }

        public static class AccountBlocked
        {
            public const string Subject = "[FIRA] Your account has been blocked";
            public const string Template = "AccountBlocked.cshtml";
        }

        public static class AccountReactivated
        {
            public const string Subject = "[FIRA] Your account has been reactivated";
            public const string Template = "AccountReactivated.cshtml";
        }

        public static class WorkspaceSuspended
        {
            public const string Subject = "[FIRA] Your workspace has been suspended";
            public const string Template = "WorkspaceSuspended.cshtml";
        }

        public static class WorkspaceReactivated
        {
            public const string Subject = "[FIRA] Your workspace has been reactivated";
            public const string Template = "WorkspaceReactivated.cshtml";
        }
    }
}