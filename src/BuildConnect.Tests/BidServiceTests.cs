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
        var service = new BidService(repository, new FakeJobRepository([TestData.CreateJob(investitorId: "investitor-1")]));

        var result = service.AcceptBid("bid-1", new RequestUserContext("investitor-1", BuildConnectRoles.Investitor));

        var acceptedBid = result.Single(bid => bid.Id == "bid-1");
        var rejectedBid = result.Single(bid => bid.Id == "bid-2");

        Assert.Equal(BidStatuses.Accepted, acceptedBid.Status);
        Assert.Equal(BidStatuses.Rejected, rejectedBid.Status);
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
}
