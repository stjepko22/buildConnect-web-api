namespace BuildConnect.DAL.Entities;

public sealed class UserProfileImageEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }

    public UserEntity User { get; set; } = null!;
}
