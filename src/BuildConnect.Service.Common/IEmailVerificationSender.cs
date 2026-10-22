namespace BuildConnect.Service.Common;

public interface IEmailVerificationSender
{
    bool SendVerificationEmail(string email, string displayName, string token);
}
