using BuildConnect.Model;
using BuildConnect.Service;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class ReviewServiceTests
{
    [Fact]
    public void CreateReview_Throws_WhenAcceptedBidDoesNotExist()
    {
        var service = new ReviewService(
            new FakeReviewRepository(),
            new FakeJobRepository([TestData.CreateJob(investitorId: "investitor-1", status: JobStatuses.Completed)]),
            new FakeBidRepository([TestData.CreateBid(status: BidStatuses.Pending)]));

        var action = () => service.CreateReview(
            "posao-1",
            new CreateReviewRequest(5, "Vrlo dobro odradjen posao."),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Recenziju mozete ostaviti tek nakon prihvacene ponude.", exception.Message);
    }

    [Fact]
    public void CreateReview_CreatesReview_ForCompletedJobAndAcceptedBid()
    {
        var repository = new FakeReviewRepository();
        var job = TestData.CreateJob(investitorId: "investitor-1", status: JobStatuses.Completed);
        var jobRepository = new FakeJobRepository([job]);
        var service = new ReviewService(
            repository,
            jobRepository,
            new FakeBidRepository([TestData.CreateBid(contractorId: "izvodjac-5", status: BidStatuses.Accepted)]));

        var result = service.CreateReview(
            "posao-1",
            new CreateReviewRequest(5, "Vrlo kvalitetno i profesionalno odradjen posao."),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.Equal("izvodjac-5", result.RevieweeId);
        Assert.Equal("investitor-1", result.ReviewerId);
        Assert.Single(repository.GetAll());
        Assert.Equal(job, jobRepository.GetById("posao-1"));
    }

    [Theory]
    [InlineData(JobStatuses.Open)]
    [InlineData(JobStatuses.Closed)]
    [InlineData(JobStatuses.InProgress)]
    public void CreateReview_ThrowsWithoutChangingJobOrSavingReview_WhenJobIsNotCompleted(string status)
    {
        var repository = new FakeReviewRepository();
        var job = TestData.CreateJob(status: status);
        var jobRepository = new FakeJobRepository([job]);
        var service = new ReviewService(
            repository,
            jobRepository,
            new FakeBidRepository([TestData.CreateBid(status: BidStatuses.Accepted)]));

        var action = () => service.CreateReview(
            job.Id,
            new CreateReviewRequest(5, "Vrlo dobro odradjen posao."),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Recenziju mozete ostaviti tek nakon zavrsetka posla.", exception.Message);
        Assert.Empty(repository.GetAll());
        Assert.Equal(job, jobRepository.GetById(job.Id));
    }

    [Theory]
    [InlineData("investitor-2", BuildConnectRoles.Investitor)]
    [InlineData("izvodjac-1", BuildConnectRoles.Izvodjac)]
    public void CreateReview_Throws_WhenUserIsNotJobOwner(string userId, string role)
    {
        var repository = new FakeReviewRepository();
        var service = new ReviewService(
            repository,
            new FakeJobRepository([TestData.CreateJob(status: JobStatuses.Completed)]),
            new FakeBidRepository([TestData.CreateBid(status: BidStatuses.Accepted)]));

        var action = () => service.CreateReview(
            "posao-1",
            new CreateReviewRequest(5, "Vrlo dobro odradjen posao."),
            new RequestUserContext(userId, role));

        Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void CreateReview_Throws_WhenReviewAlreadyExists()
    {
        var existingReview = TestData.CreateReview();
        var repository = new FakeReviewRepository([existingReview]);
        var service = new ReviewService(
            repository,
            new FakeJobRepository([TestData.CreateJob(status: JobStatuses.Completed)]),
            new FakeBidRepository([TestData.CreateBid(status: BidStatuses.Accepted)]));

        var action = () => service.CreateReview(
            "posao-1",
            new CreateReviewRequest(5, "Vrlo dobro odradjen posao."),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Za ovaj posao je vec ostavljena recenzija.", exception.Message);
        Assert.Equal(existingReview, Assert.Single(repository.GetAll()));
    }
}
