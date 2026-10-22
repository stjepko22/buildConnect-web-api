using BuildConnect.Model;
using BuildConnect.Service;
using BuildConnect.Tests.Support;
using Microsoft.AspNetCore.Identity;

namespace BuildConnect.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public void Login_ReturnsUser_WhenCredentialsAreValid()
    {
        var authData = TestData.CreateAuthUser(BuildConnectRoles.Investitor, "investitor@test.hr", "secret123");
        var service = new AuthService(
            new FakeAuthRepository([authData.account]),
            new FakeUserRepository([authData.user]),
            new FakeEmailVerificationSender(),
            new PasswordHasher<AuthAccount>());

        var result = service.Login(new LoginRequest(authData.user.Email, authData.plainPassword));

        Assert.Equal(authData.user.Id, result.Id);
        Assert.Equal(authData.user.Email, result.Email);
        Assert.Equal(BuildConnectRoles.Investitor, result.Role);
    }

    [Fact]
    public void Login_ThrowsUnauthorized_WhenPasswordIsInvalid()
    {
        var authData = TestData.CreateAuthUser(BuildConnectRoles.Investitor, "investitor@test.hr", "secret123");
        var service = new AuthService(
            new FakeAuthRepository([authData.account]),
            new FakeUserRepository([authData.user]),
            new FakeEmailVerificationSender(),
            new PasswordHasher<AuthAccount>());

        var action = () => service.Login(new LoginRequest(authData.user.Email, "wrong-password"));

        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Equal("Neispravna email adresa ili lozinka.", exception.Message);
    }

    [Fact]
    public void Login_ThrowsUnauthorized_WhenEmailIsNotVerified()
    {
        var authData = TestData.CreateAuthUser(BuildConnectRoles.Investitor, "investitor@test.hr", "secret123");
        var account = authData.account with { IsEmailVerified = false };
        var service = new AuthService(
            new FakeAuthRepository([account]),
            new FakeUserRepository([authData.user]),
            new FakeEmailVerificationSender(),
            new PasswordHasher<AuthAccount>());

        var action = () => service.Login(new LoginRequest(authData.user.Email, authData.plainPassword));

        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Equal("Email adresa nije potvrdjena. Provjerite svoj inbox.", exception.Message);
    }

    [Fact]
    public void Register_CreatesHashedPasswordAndSendsVerificationEmail_ForNewContractor()
    {
        var userRepository = new FakeUserRepository();
        var emailVerificationSender = new FakeEmailVerificationSender();
        var service = new AuthService(
            new FakeAuthRepository(),
            userRepository,
            emailVerificationSender,
            new PasswordHasher<AuthAccount>());

        var result = service.Register(new RegisterRequest(
            "Ivan",
            "Ivic",
            "izvodjac@test.hr",
            "secret123",
            "0912345678",
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.Firma));

        var createdUser = userRepository.GetByEmail(result.Email);
        var storedPasswordHash = userRepository.GetStoredPasswordHash(createdUser!.Id);

        Assert.NotNull(createdUser);
        Assert.Equal("Ivan Ivic", createdUser!.DisplayName);
        Assert.Equal(BuildConnectRoles.Izvodjac, createdUser.Role);
        Assert.Equal("0912345678", createdUser.Phone);
        Assert.False(createdUser.IsPhoneVisible);
        Assert.Equal("izvodjac@test.hr", result.Email);
        Assert.True(result.IsVerificationEmailSent);
        Assert.Single(emailVerificationSender.SentEmails);
        Assert.False(string.IsNullOrWhiteSpace(storedPasswordHash));
        Assert.DoesNotContain("secret123", storedPasswordHash!, StringComparison.Ordinal);
    }

    [Fact]
    public void Register_RejectsDuplicateEmail_WhenOnlyCasingDiffers()
    {
        var authData = TestData.CreateAuthUser(BuildConnectRoles.Investitor, "investitor@test.hr", "secret123");
        var service = new AuthService(
            new FakeAuthRepository([authData.account]),
            new FakeUserRepository([authData.user]),
            new FakeEmailVerificationSender(),
            new PasswordHasher<AuthAccount>());

        var action = () => service.Register(new RegisterRequest(
            "Ana",
            "Anic",
            "INVESTITOR@Test.hr",
            "secret123",
            "0911112222",
            BuildConnectRoles.Investitor,
            null));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Korisnik s ovom email adresom vec postoji.", exception.Message);
    }

    [Theory]
    [InlineData(null, "Kontakt broj je obavezan.")]
    [InlineData("", "Kontakt broj je obavezan.")]
    [InlineData("   ", "Kontakt broj je obavezan.")]
    [InlineData(" 12345 ", "Kontakt broj mora imati barem 6 znakova.")]
    [InlineData("123456789012345678901234567890123", "Kontakt broj moze imati najvise 32 znaka.")]
    public void Register_RejectsInvalidPhone_BeforeCreatingUserOrSendingEmail(string? phone, string expectedMessage)
    {
        var userRepository = new FakeUserRepository();
        var emailVerificationSender = new FakeEmailVerificationSender();
        var service = new AuthService(
            new FakeAuthRepository(),
            userRepository,
            emailVerificationSender,
            new PasswordHasher<AuthAccount>());

        var action = () => service.Register(new RegisterRequest(
            "Ivan", "Ivic", "izvodjac@test.hr", "secret123", phone,
            BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma));

        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal(expectedMessage, exception.Message);
        Assert.Empty(userRepository.GetAll());
        Assert.Empty(emailVerificationSender.SentEmails);
    }

    [Theory]
    [InlineData(" 123456 ", "123456")]
    [InlineData(" 12345678901234567890123456789012 ", "12345678901234567890123456789012")]
    public void Register_AcceptsAndTrimsPhone_AtLengthBoundaries(string phone, string expectedPhone)
    {
        var userRepository = new FakeUserRepository();
        var emailVerificationSender = new FakeEmailVerificationSender();
        var service = new AuthService(
            new FakeAuthRepository(),
            userRepository,
            emailVerificationSender,
            new PasswordHasher<AuthAccount>());

        var result = service.Register(new RegisterRequest(
            "Ivan", "Ivic", "izvodjac@test.hr", "secret123", phone,
            BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma));

        var user = Assert.Single(userRepository.GetAll());
        Assert.Equal(expectedPhone, user.Phone);
        Assert.True(result.IsVerificationEmailSent);
        Assert.Single(emailVerificationSender.SentEmails);
    }

    [Fact]
    public void Register_ReturnsUnsentStatusAndKeepsUser_WhenVerificationEmailCannotBeSent()
    {
        var userRepository = new FakeUserRepository();
        var emailVerificationSender = new FakeEmailVerificationSender { IsSuccessful = false };
        var service = new AuthService(
            new FakeAuthRepository(),
            userRepository,
            emailVerificationSender,
            new PasswordHasher<AuthAccount>());

        var result = service.Register(new RegisterRequest(
            "Ivan", "Ivic", "izvodjac@test.hr", "secret123", "0912345678",
            BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma));

        Assert.False(result.IsVerificationEmailSent);
        Assert.NotNull(userRepository.GetByEmail(result.Email));
        Assert.Single(emailVerificationSender.SentEmails);
    }

    [Fact]
    public void ResendVerificationEmail_ReturnsUnsentStatus_WhenVerificationEmailCannotBeSent()
    {
        var authData = TestData.CreateAuthUser(BuildConnectRoles.Investitor, "investitor@test.hr", "secret123");
        var account = authData.account with { IsEmailVerified = false };
        var emailVerificationSender = new FakeEmailVerificationSender { IsSuccessful = false };
        var service = new AuthService(
            new FakeAuthRepository([account]),
            new FakeUserRepository([authData.user]),
            emailVerificationSender,
            new PasswordHasher<AuthAccount>());

        var result = service.ResendVerificationEmail(new ResendVerificationEmailRequest(authData.user.Email));

        Assert.Equal(authData.user.Email, result.Email);
        Assert.False(result.IsVerificationEmailSent);
        Assert.Single(emailVerificationSender.SentEmails);
    }
}
