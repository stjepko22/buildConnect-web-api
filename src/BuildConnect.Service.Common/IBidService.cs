using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IBidService
{
    IReadOnlyCollection<BidResponse> GetBids(string? jobId = null, string? contractorId = null);

    BidResponse CreateBid(string jobId, CreateBidRequest request, RequestUserContext userContext);

    IReadOnlyCollection<BidResponse> AcceptBid(string bidId, RequestUserContext userContext);
}
