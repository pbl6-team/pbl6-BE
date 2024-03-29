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

    public enum NOTIFICATION_STATUS
    {
        PENDING = 1,
        SENT = 2,
        READ = 3,
        FAILED = 4
    }

    public enum NOTIFICATION_TYPE
    {
        GENERAL = 1,
        NEW_MESSAGE = 2,
        FRIEND_REQUEST = 3,
        CHANNEL_INVITATION = 4,
        CHANNEL_REMOVED = 5,
        WORKSPACE_INVITATION = 6,
        WORKSPACE_REMOVED = 7,
        MEETING = 8,
    }

    public enum WORKSPACE_STATUS
    {
        ACTIVE = 1,
        SUSPENDED = 2,
    }

    public enum WORKSPACE_MEMBER_STATUS
    {
        ACTIVE = 1,
        SUSPENDED = 2,
        INVITED = 3,
        REMOVED = 4,
        DECLINED = 5,
    }

    public enum CHANNEL_MEMBER_STATUS
    {
        ACTIVE = 1,
        SUSPENDED = 2,
        INVITED = 3,
        REMOVED = 4,
        DECLINED = 5,
    }

    public enum CHANNEL_CATEGORY
    {
        DEFAULT = 1,
        PRIVATE = 2,
        PUBLIC = 3,
        MEETING = 4,
    }

    public enum ADMIN_STATUS
    {
        ACTIVE = 1,
        BLOCKED = 2,
    }

    public enum MESSAGE_TYPE
    {
        DEFAULT = 0,
        MEETING = 1,
        SYSTEM = 2,
        CALL = 3,
    }

    public enum MEETING_TYPE
    {
        PRIVATE = 0,
        PUBLIC = 1,
    }

    public enum CALL_STATUS
    {
        ACTIVE = 1,
        ENDED = 2,
    }

    public enum MEETING_STATUS
    {
        SCHEDULED = 0,
        ACTIVE = 1,
        ENDED = 2,
        CANCELED = 3,
    }

    public enum FILE_TYPE
    {
        MEDIA = 1,
        DOCUMENT = 2,
    }
}