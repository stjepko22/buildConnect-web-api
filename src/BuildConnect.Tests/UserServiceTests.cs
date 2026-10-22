using BuildConnect.Model;
using BuildConnect.Service;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class UserServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PublicProfileQueries_RespectPhoneVisibility(bool isPhoneVisible)
    {
        var contractor = TestData.CreateIzvodjac() with
        {
            Phone = "0951234567",
            IsPhoneVisible = isPhoneVisible
        };
        var service = new UserService(new FakeUserRepository([contractor]));

        var responses = new[]
        {
            Assert.Single(service.GetUsers()),
            Assert.Single(service.GetContractors()),
            service.GetUserById(contractor.Id)!
        };

        Assert.All(responses, response =>
        {
            Assert.Equal(isPhoneVisible ? contractor.Phone : null, response.Phone);
            Assert.Equal(isPhoneVisible, response.IsPhoneVisible);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetUserById_ReturnsHiddenPhone_OnlyToOwner(bool isOwner)
    {
        var contractor = TestData.CreateIzvodjac() with
        {
            Phone = "0951234567",
            IsPhoneVisible = false
        };
        var service = new UserService(new FakeUserRepository([contractor]));
        var userContext = new RequestUserContext(isOwner ? contractor.Id : "other-user", contractor.Role);

        var response = service.GetUserById(contractor.Id, userContext);

        Assert.NotNull(response);
        Assert.Equal(isOwner ? contractor.Phone : null, response.Phone);
        Assert.False(response.IsPhoneVisible);
    }

    [Fact]
    public void PublicProfileQueries_DisableVisibility_WhenPhoneIsMissing()
    {
        var contractor = TestData.CreateIzvodjac() with { Phone = null, IsPhoneVisible = true };
        var service = new UserService(new FakeUserRepository([contractor]));

        var responses = new[]
        {
            Assert.Single(service.GetUsers()),
            Assert.Single(service.GetContractors()),
            service.GetUserById(contractor.Id)!
        };

        Assert.All(responses, response =>
        {
            Assert.Null(response.Phone);
            Assert.False(response.IsPhoneVisible);
        });
    }

    [Fact]
    public void GetContractors_FiltersContractors_ByQueryCategoryLocationAndLegalType()
    {
        var repository = new FakeUserRepository(
        [
            TestData.CreateIzvodjac(id: "izvodjac-1", displayName: "Fasada Master") with
            {
                Location = "Zagreb",
                LegalType = BuildConnectLegalTypes.Firma,
                ServiceCategories = ["Fasade", "Renovacija"]
            },
            TestData.CreateIzvodjac(id: "izvodjac-2", displayName: "Keramika Pro") with
            {
                Location = "Split",
                LegalType = BuildConnectLegalTypes.FizickaOsoba,
                ServiceCategories = ["Keramika"]
            },
            TestData.CreateIzvodjac(id: "izvodjac-3", displayName: "Fasada Zagreb") with
            {
                Location = "Zagreb",
                LegalType = BuildConnectLegalTypes.Firma,
                ServiceCategories = ["Fasade"]
            }
        ]);
        var service = new UserService(repository);

        var results = service.GetContractors(new GetContractorsRequest("fasad", ["Fasade"], "Zagreb", BuildConnectLegalTypes.Firma));

        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.Equal(BuildConnectLegalTypes.Firma, result.LegalType));
        Assert.All(results, result => Assert.Equal("Zagreb", result.Location));
        Assert.All(results, result => Assert.Contains("Fasade", result.ServiceCategories ?? []));
    }

    [Fact]
    public void UpdateCurrentUserProfile_UpdatesContractorDetails()
    {
        var contractor = TestData.CreateIzvodjac(
            id: "izvodjac-1",
            email: "izvodjac@test.hr",
            displayName: "Stari Izvodjac") with
        {
            Phone = "0951234567",
            IsPhoneVisible = false,
            Bio = "Stari opis",
            Location = "Split",
            ServiceCategories = ["Fasade"]
        };
        var repository = new FakeUserRepository([contractor]);
        var service = new UserService(repository);

        var response = service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(
                "Novi Izvodjac",
                BuildConnectLegalTypes.FizickaOsoba,
                "Novi opis profila",
                "Zagreb",
                ["Elektro", "Fasade"],
                "0912345678",
                true),
            new RequestUserContext(contractor.Id, contractor.Role, contractor.DisplayName));

        Assert.Equal("Novi Izvodjac", response.DisplayName);
        Assert.Equal(BuildConnectLegalTypes.FizickaOsoba, response.LegalType);
        Assert.Equal("0912345678", response.Phone);
        Assert.True(response.IsPhoneVisible);
        Assert.Equal("Novi opis profila", response.Bio);
        Assert.Equal("Zagreb", response.Location);
        Assert.Equal(["Elektro", "Fasade"], response.ServiceCategories);
    }

    [Fact]
    public void UpdateCurrentUserProfile_ClearsServiceCategories_ForInvestor()
    {
        var investor = TestData.CreateInvestitor(
            id: "investitor-1",
            email: "investitor@test.hr",
            displayName: "Investitor") with
        {
            Phone = "0950000000",
            IsPhoneVisible = true,
            Bio = "Opis",
            Location = "Mostar",
            ServiceCategories = ["Gradnja"]
        };
        var repository = new FakeUserRepository([investor]);
        var service = new UserService(repository);

        var response = service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(
                "Investitor Plus",
                BuildConnectLegalTypes.Firma,
                "Novi opis",
                "Sarajevo",
                ["Gradnja", "Keramika"],
                "0919988776",
                true),
            new RequestUserContext(investor.Id, investor.Role, investor.DisplayName));

        Assert.Equal("Investitor Plus", response.DisplayName);
        Assert.Equal("0919988776", response.Phone);
        Assert.True(response.IsPhoneVisible);
        Assert.Null(response.ServiceCategories);
    }

    [Fact]
    public void UpdateCurrentUserProfile_ReturnsHiddenPhone_ToOwner()
    {
        var contractor = TestData.CreateIzvodjac();
        var repository = new FakeUserRepository([contractor]);
        var service = new UserService(repository);

        var response = service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(
                contractor.DisplayName, contractor.LegalType, contractor.Bio, contractor.Location,
                Phone: " 0951234567 ", IsPhoneVisible: false),
            new RequestUserContext(contractor.Id, contractor.Role));

        Assert.Equal("0951234567", response.Phone);
        Assert.False(response.IsPhoneVisible);
        Assert.Equal("0951234567", repository.GetById(contractor.Id)!.Phone);
        Assert.Null(service.GetUserById(contractor.Id)!.Phone);
    }

    [Fact]
    public void UpdateCurrentUserProfile_PreservesPhone_WhenOmitted()
    {
        var contractor = TestData.CreateIzvodjac() with { Phone = "0951234567", IsPhoneVisible = false };
        var repository = new FakeUserRepository([contractor]);
        var service = new UserService(repository);

        var response = service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(contractor.DisplayName, contractor.LegalType, contractor.Bio, contractor.Location),
            new RequestUserContext(contractor.Id, contractor.Role));

        Assert.Equal(contractor.Phone, response.Phone);
        Assert.Equal(contractor.Phone, repository.GetById(contractor.Id)!.Phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCurrentUserProfile_ClearsPhoneAndVisibility_WhenExplicitlyEmpty(string phone)
    {
        var contractor = TestData.CreateIzvodjac() with { Phone = "0951234567", IsPhoneVisible = true };
        var repository = new FakeUserRepository([contractor]);
        var service = new UserService(repository);

        var response = service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(
                contractor.DisplayName, contractor.LegalType, contractor.Bio, contractor.Location,
                Phone: phone, IsPhoneVisible: true),
            new RequestUserContext(contractor.Id, contractor.Role));

        Assert.Null(response.Phone);
        Assert.False(response.IsPhoneVisible);
        var savedUser = repository.GetById(contractor.Id)!;
        Assert.Null(savedUser.Phone);
        Assert.False(savedUser.IsPhoneVisible);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(33)]
    public void UpdateCurrentUserProfile_RejectsPhoneOutsideAllowedLength(int length)
    {
        var contractor = TestData.CreateIzvodjac() with { Phone = "0951234567" };
        var repository = new FakeUserRepository([contractor]);
        var service = new UserService(repository);

        Assert.Throws<ArgumentException>(() => service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(
                contractor.DisplayName, contractor.LegalType, contractor.Bio, contractor.Location,
                Phone: new string('1', length)),
            new RequestUserContext(contractor.Id, contractor.Role)));

        Assert.Equal(contractor.Phone, repository.GetById(contractor.Id)!.Phone);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(32)]
    public void UpdateCurrentUserProfile_AcceptsTrimmedPhoneAtLengthBoundaries(int length)
    {
        var contractor = TestData.CreateIzvodjac();
        var service = new UserService(new FakeUserRepository([contractor]));
        var phone = new string('1', length);

        var response = service.UpdateCurrentUserProfile(
            new UpdateUserProfileRequest(
                contractor.DisplayName, contractor.LegalType, contractor.Bio, contractor.Location,
                Phone: $" {phone} ", IsPhoneVisible: true),
            new RequestUserContext(contractor.Id, contractor.Role));

        Assert.Equal(phone, response.Phone);
        Assert.True(response.IsPhoneVisible);
    }
}
