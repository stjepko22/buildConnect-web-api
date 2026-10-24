using BuildConnect.Model;
using BuildConnect.Service;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class BidServiceTests
{
    [Fact]
    public void CreateBid_Throws_WhenContractorAlreadyHasActiveBid()
    {
        var service = new BidService(
            new FakeBidRepository([TestData.CreateBid(contractorId: "izvodjac-1", status: BidStatuses.Pending)]),
            new FakeJobRepository([TestData.CreateJob()]));

        var action = () => service.CreateBid(
            "posao-1",
            new CreateBidRequest(2500m, 10, "Mozemo preuzeti posao vrlo brzo."),
            new RequestUserContext("izvodjac-1", BuildConnectRoles.Izvodjac, "Ivan Ivic"));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Vec imate aktivnu ponudu za ovaj posao.", exception.Message);
    }

    [Fact]
    public void AcceptBid_AcceptsSelectedBid_AndRejectsOthers()
    {
        var selectedBid = TestData.CreateBid(id: "bid-1", contractorId: "izvodjac-1");
        var otherBid = TestData.CreateBid(id: "bid-2", contractorId: "izvodjac-2");
        var repository = new FakeBidRepository([selectedBid, otherBid]);
        var jobRepository = new FakeJobRepository([TestData.CreateJob(investitorId: "investitor-1")]);
        var service = new BidService(repository, jobRepository);

        var result = service.AcceptBid("bid-1", new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var acceptedBid = result.Single(bid => bid.Id == "bid-1");
        var rejectedBid = result.Single(bid => bid.Id == "bid-2");

        Assert.Equal(BidStatuses.Accepted, acceptedBid.Status);
        Assert.Equal(BidStatuses.Rejected, rejectedBid.Status);
        Assert.Equal(JobStatuses.InProgress, jobRepository.GetById("posao-1")!.Status);
    }

    [Theory]
    [InlineData(JobStatuses.Closed)]
    [InlineData(JobStatuses.InProgress)]
    [InlineData(JobStatuses.Completed)]
    public void AcceptBid_ThrowsWithoutChangingBidsOrJob_WhenJobIsNotOpen(string status)
    {
        var selectedBid = TestData.CreateBid(id: "bid-1", contractorId: "izvodjac-1");
        var otherBid = TestData.CreateBid(id: "bid-2", contractorId: "izvodjac-2");
        var repository = new FakeBidRepository([selectedBid, otherBid]);
        var job = TestData.CreateJob(status: status);
        var jobRepository = new FakeJobRepository([job]);
        var service = new BidService(repository, jobRepository);

        var action = () => service.AcceptBid("bid-1", new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Ponude je moguce prihvatiti samo na otvorene oglase.", exception.Message);
        Assert.Equal(selectedBid, repository.GetById("bid-1"));
        Assert.Equal(otherBid, repository.GetById("bid-2"));
        Assert.Equal(job, jobRepository.GetById("posao-1"));
    }

    [Theory]
    [InlineData(JobStatuses.InProgress)]
    [InlineData(JobStatuses.Completed)]
    public void AcceptBid_ReturnsExistingResultWithoutChangingJob_WhenSameBidIsAlreadyAccepted(string status)
    {
        var acceptedBid = TestData.CreateBid(id: "bid-1", status: BidStatuses.Accepted);
        var rejectedBid = TestData.CreateBid(id: "bid-2", contractorId: "izvodjac-2", status: BidStatuses.Rejected);
        var repository = new FakeBidRepository([acceptedBid, rejectedBid]);
        var job = TestData.CreateJob(status: status);
        var jobRepository = new FakeJobRepository([job]);
        var service = new BidService(repository, jobRepository);

        var result = service.AcceptBid("bid-1", new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        Assert.Equal(2, result.Count);
        Assert.Equal(BidStatuses.Accepted, result.Single(bid => bid.Id == "bid-1").Status);
        Assert.Equal(BidStatuses.Rejected, result.Single(bid => bid.Id == "bid-2").Status);
        Assert.Equal(acceptedBid, repository.GetById("bid-1"));
        Assert.Equal(rejectedBid, repository.GetById("bid-2"));
        Assert.Equal(job, jobRepository.GetById("posao-1"));
    }

    [Fact]
    public void AcceptBid_Throws_WhenAnotherBidIsAlreadyAccepted()
    {
        var acceptedBid = TestData.CreateBid(id: "bid-1", contractorId: "izvodjac-1", status: BidStatuses.Accepted);
        var pendingBid = TestData.CreateBid(id: "bid-2", contractorId: "izvodjac-2", status: BidStatuses.Pending);
        var service = new BidService(
            new FakeBidRepository([acceptedBid, pendingBid]),
            new FakeJobRepository([TestData.CreateJob(investitorId: "investitor-1")]));

        var action = () => service.AcceptBid("bid-2", new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Za ovaj posao je vec odabrana ponuda.", exception.Message);
    }

    [Fact]
    public void CreateBid_Throws_WhenJobIsClosed()
    {
        var service = new BidService(
            new FakeBidRepository(),
            new FakeJobRepository([TestData.CreateJob(status: JobStatuses.Closed)]));

        var action = () => service.CreateBid(
            "posao-1",
            new CreateBidRequest(2500m, 10, "Mozemo preuzeti posao vrlo brzo."),
            new RequestUserContext("izvodjac-1", BuildConnectRoles.Izvodjac, "Ivan Ivic"));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Ponude je moguce slati samo na otvorene oglase.", exception.Message);
    }

    [Fact]
    public void CreateBid_Throws_WhenContractorTriesToBidOnOwnJob()
    {
        var service = new BidService(
            new FakeBidRepository(),
            new FakeJobRepository([TestData.CreateJob(investitorId: "izvodjac-1")]));

        var action = () => service.CreateBid(
            "posao-1",
            new CreateBidRequest(2500m, 10, "Mozemo preuzeti posao vrlo brzo."),
            new RequestUserContext("izvodjac-1", BuildConnectRoles.Izvodjac, "Ivan Ivic"));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Ne mozete poslati ponudu na vlastiti oglas.", exception.Message);
    }
}
