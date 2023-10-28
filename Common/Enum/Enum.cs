namespace PBL6.Common.Enum
{
    public enum USER
    {
        DEFAULT = 0,
        VERIFIED,
        BLOCKED,
        SUSPENDED,
        PREMIUM,
    }
    
    public enum OTP_TYPE
    {
        VERIFY_USER = 1,
        CHANGE_PASSWORD,
        FORGOT_PASSWORD
    }
}