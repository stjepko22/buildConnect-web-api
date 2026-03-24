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
            new FakeJobRepository([TestData.CreateJob(investitorId: "investitor-1")]),
            new FakeBidRepository([TestData.CreateBid(status: BidStatuses.Pending)]));

        var action = () => service.CreateReview(
            "posao-1",
            new CreateReviewRequest(5, "Vrlo dobro odradjen posao."),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Recenziju mozete ostaviti tek nakon prihvacene ponude.", exception.Message);
    }

    [Fact]
    public void CreateReview_CreatesReview_ForAcceptedBid()
    {
        var repository = new FakeReviewRepository();
        var service = new ReviewService(
            repository,
            new FakeJobRepository([TestData.CreateJob(investitorId: "investitor-1")]),
            new FakeBidRepository([TestData.CreateBid(contractorId: "izvodjac-5", status: BidStatuses.Accepted)]));

        var result = service.CreateReview(
            "posao-1",
            new CreateReviewRequest(5, "Vrlo kvalitetno i profesionalno odradjen posao."),
            new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.Equal("izvodjac-5", result.RevieweeId);
        Assert.Single(repository.GetAll());
    }
}
