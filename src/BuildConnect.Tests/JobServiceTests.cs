using BuildConnect.Model;
using BuildConnect.Service;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class JobServiceTests
{
    [Fact]
    public void CreateJob_ThrowsForbidden_WhenUserIsNotInvestitor()
    {
        var service = new JobService(new FakeJobRepository());

        var action = () => service.CreateJob(
            new CreateJobRequest("Naslov", "Detaljan opis oglasa", "Fasade", "Zagreb", 2000m, "2026-05-01"),
            new RequestUserContext("izvodjac-1", BuildConnectRoles.Izvodjac));

        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Equal("Samo investitori mogu kreirati oglas.", exception.Message);
    }

    [Fact]
    public void CreateJob_CreatesJob_ForInvestitor()
    {
        var repository = new FakeJobRepository();
        var service = new JobService(repository);

        var result = service.CreateJob(
            new CreateJobRequest("Naslov", "Detaljan opis oglasa", "Fasade", "Zagreb", 2000m, "2026-05-01"),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.Equal("Naslov", result.Title);
        Assert.Equal("investitor-1", result.InvestitorId);
        Assert.Single(repository.GetAll());
    }
}
