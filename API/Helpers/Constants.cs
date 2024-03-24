namespace API.Helpers;

public static class Constants
{
    public static class Roles
    {
        public const string Member = "Member";
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
    }

    public static class Gender
    {
        public const string Male = "male";
        public const string Female = "female";
    }

    public static class LikePredicates
    {
        public const string Liked = "liked";
        public const string LikedBy = "likedBy";
    }

    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string ModeratePhotoRole = "ModeratePhotoRole";
    }

    public static class SignalRMessages
    {
        public const string UserIsOnline = "UserIsOnline";
        public const string UserIsOffline = "UserIsOffline";
        public const string GetOnlineUsers = "GetOnlineUsers";
        public const string ReceiveMessageThread = "ReceiveMessageThread";
        public const string UpdateGroup = "UpdateGroup";
        public const string NewMessage = "NewMessage";
        public const string NewMessageReceived = "NewMessageReceived";
    }
}
