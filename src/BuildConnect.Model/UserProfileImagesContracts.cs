namespace BuildConnect.Model;

public static class UserProfileImageKinds
{
    public const string Profile = "profile";
    public const string Cover = "cover";

    public static bool IsSupported(string kind) => kind is Profile or Cover;
}

public static class UserProfileImageLimits
{
    public const int MaxFileSize = 5 * 1024 * 1024;
}

public sealed record UserProfileImage(byte[] Content, string ContentType, string Version);
