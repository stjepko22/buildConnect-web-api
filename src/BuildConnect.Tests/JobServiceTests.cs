using BuildConnect.Model;
using BuildConnect.Service;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class JobServiceTests
{
    [Fact]
    public void CreateJob_ThrowsForbidden_WhenUserIsNotInvestitor()
    {
        var service = new JobService(new FakeJobRepository(), new FakeBidRepository());

        var action = () => service.CreateJob(
            new CreateJobRequest("Naslov", "Detaljan opis oglasa", "Fasade", "Zagreb", 2000m, "2026-05-01"),
            new RequestUserContext("izvodjac-1", BuildConnectRoles.Izvodjac));

        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Equal("Samo investitori mogu upravljati oglasima.", exception.Message);
    }

    [Fact]
    public void GetJobs_FiltersJobs_ByQueryAndCategory()
    {
        var repository = new FakeJobRepository(
        [
            TestData.CreateJob(id: "posao-1"),
            TestData.CreateJob(id: "posao-2") with
            {
                Title = "Keramika za kupaonicu",
                Description = "Postavljanje plocica u novogradnji.",
                Category = "Keramika",
                Location = "Split"
            },
            TestData.CreateJob(id: "posao-3") with
            {
                Title = "Fasaderski radovi na poslovnom objektu",
                Category = "Fasade",
                Location = "Rijeka"
            }
        ]);
        var service = new JobService(repository, new FakeBidRepository());

        var results = service.GetJobs(new GetJobsRequest("fasad", ["Fasade"]));

        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.Equal("Fasade", result.Category));
        Assert.All(results, result =>
            Assert.Contains(
                "fasad",
                string.Join(' ', result.Title, result.Description, result.Location, result.Category),
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateJob_CreatesJob_ForInvestitor()
    {
        var repository = new FakeJobRepository();
        var service = new JobService(repository, new FakeBidRepository());

        var result = service.CreateJob(
            new CreateJobRequest("Naslov", "Detaljan opis oglasa", "Fasade", "Zagreb", 2000m, "2026-05-01"),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.Equal("Naslov", result.Title);
        Assert.Equal("investitor-1", result.InvestitorId);
        Assert.Equal(JobStatuses.Open, result.Status);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void UpdateJob_UpdatesExistingJob_ForOwner()
    {
        var repository = new FakeJobRepository(
        [
            new Job(
                "posao-1",
                "Stari naslov",
                "Stari opis oglasa",
                "Fasade",
                "Zagreb",
                2000m,
                "2026-05-01",
                "investitor-1",
                new DateTimeOffset(2026, 2, 8, 0, 0, 0, TimeSpan.Zero),
                JobStatuses.Open)
        ]);
        var service = new JobService(repository, new FakeBidRepository());

        var result = service.UpdateJob(
            "posao-1",
            new UpdateJobRequest("Novi naslov", "Novi detaljan opis", "Renovacija", "Split", 3500m, "2026-07-01"),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.NotNull(result);
        Assert.Equal("Novi naslov", result!.Title);
        Assert.Equal("Renovacija", result.Category);
        Assert.Equal("Split", result.Location);
    }

    [Fact]
    public void UpdateJob_ThrowsForbidden_WhenUserIsNotOwner()
    {
        var repository = new FakeJobRepository(
        [
            new Job(
                "posao-1",
                "Naslov",
                "Detaljan opis oglasa",
                "Fasade",
                "Zagreb",
                2000m,
                "2026-05-01",
                "investitor-1",
                new DateTimeOffset(2026, 2, 8, 0, 0, 0, TimeSpan.Zero),
                JobStatuses.Open)
        ]);
        var service = new JobService(repository, new FakeBidRepository());

        var action = () => service.UpdateJob(
            "posao-1",
            new UpdateJobRequest("Novi naslov", "Novi detaljan opis", "Fasade", "Zagreb", 2500m, "2026-06-01"),
            new RequestUserContext("investitor-2", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Equal("Mozete uredjivati samo vlastite oglase.", exception.Message);
    }

    [Fact]
    public void CloseJob_ClosesOwnedOpenJob_WithoutAcceptedBid()
    {
        var repository = new FakeJobRepository([TestData.CreateJob()]);
        var service = new JobService(repository, new FakeBidRepository());

        var result = service.CloseJob(
            "posao-1",
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.NotNull(result);
        Assert.Equal(JobStatuses.Closed, result!.Status);
    }

    [Fact]
    public void DeleteJob_DeletesOwnedClosedJob_WithoutAcceptedBid()
    {
        var repository = new FakeJobRepository([TestData.CreateJob(status: JobStatuses.Closed)]);
        var service = new JobService(repository, new FakeBidRepository());

        var isDeleted = service.DeleteJob(
            "posao-1",
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.True(isDeleted);
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void DeleteJob_Throws_WhenJobHasAcceptedBid()
    {
        var repository = new FakeJobRepository([TestData.CreateJob(status: JobStatuses.InProgress)]);
        var service = new JobService(
            repository,
            new FakeBidRepository([TestData.CreateBid(status: BidStatuses.Accepted)]));

        void Action() => service.DeleteJob(
            "posao-1",
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(Action);
        Assert.Equal("Oglas nije moguce obrisati nakon prihvacene ponude ili zavrsenog projekta.", exception.Message);
    }

    [Fact]
    public void CompleteJob_CompletesInProgressJob_ForAcceptedContractor()
    {
        var repository = new FakeJobRepository([TestData.CreateJob(status: JobStatuses.InProgress)]);
        var service = new JobService(
            repository,
            new FakeBidRepository([TestData.CreateBid(contractorId: "izvodjac-1", status: BidStatuses.Accepted)]));

        var result = service.CompleteJob(
            "posao-1",
            new RequestUserContext("izvodjac-1", BuildConnectRoles.Izvodjac));

        Assert.NotNull(result);
        Assert.Equal(JobStatuses.Completed, result!.Status);
    }

    [Fact]
    public void CompleteJob_Throws_WhenUserIsNotAcceptedContractor()
    {
        var repository = new FakeJobRepository([TestData.CreateJob(status: JobStatuses.InProgress)]);
        var service = new JobService(
            repository,
            new FakeBidRepository([TestData.CreateBid(contractorId: "izvodjac-1", status: BidStatuses.Accepted)]));

        void Action() => service.CompleteJob(
            "posao-1",
            new RequestUserContext("izvodjac-2", BuildConnectRoles.Izvodjac));

        var exception = Assert.Throws<UnauthorizedAccessException>(Action);
        Assert.Equal("Samo odabrani izvodjac moze oznaciti posao zavrsenim.", exception.Message);
    }
}
