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
    }
}