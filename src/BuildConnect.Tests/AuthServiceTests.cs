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
            new PasswordHasher<AuthAccount>());

        var action = () => service.Login(new LoginRequest(authData.user.Email, "wrong-password"));

        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Equal("Neispravna email adresa ili lozinka.", exception.Message);
    }

    [Fact]
    public void Register_CreatesHashedPassword_ForNewContractor()
    {
        var userRepository = new FakeUserRepository();
        var service = new AuthService(
            new FakeAuthRepository(),
            userRepository,
            new PasswordHasher<AuthAccount>());

        var result = service.Register(new RegisterRequest(
            "Ivan",
            "Ivic",
            "izvodjac@test.hr",
            "secret123",
            null,
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.Firma));

        var createdUser = userRepository.GetById(result.Id);

        Assert.NotNull(createdUser);
        Assert.Equal("Ivan Ivic", createdUser!.DisplayName);
        Assert.Equal(BuildConnectRoles.Izvodjac, createdUser.Role);
        Assert.False(string.IsNullOrWhiteSpace(createdUser.PasswordHash));
        Assert.DoesNotContain("secret123", createdUser.PasswordHash, StringComparison.Ordinal);
    }
}
