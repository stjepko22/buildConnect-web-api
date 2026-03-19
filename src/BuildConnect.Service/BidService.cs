using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IJobRepository _jobRepository;

    public BidService(IBidRepository bidRepository, IJobRepository jobRepository)
    {
        _bidRepository = bidRepository;
        _jobRepository = jobRepository;
    }

    public IReadOnlyCollection<BidResponse> GetBids(string? jobId = null, string? contractorId = null)
    {
        IReadOnlyCollection<Bid> bids;

        if (!string.IsNullOrWhiteSpace(jobId))
        {
            bids = _bidRepository.GetByJobId(jobId.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(contractorId))
        {
            bids = _bidRepository.GetByContractorId(contractorId.Trim());
        }
        else
        {
            bids = _bidRepository.GetAll();
        }

        return bids
            .Select(MapToResponse)
            .ToArray();
    }

    public BidResponse CreateBid(string jobId, CreateBidRequest request, RequestUserContext userContext)
    {
        ValidateCreateContext(userContext);
        ValidateCreateRequest(request);

        var normalizedJobId = jobId.Trim();
        var job = _jobRepository.GetById(normalizedJobId);
        if (job is null)
        {
            throw new ArgumentException("Posao nije pronadjen.");
        }

        var jobBids = _bidRepository.GetByJobId(normalizedJobId);
        var hasExistingActiveBid = jobBids.Any(bid =>
            string.Equals(bid.ContractorId, userContext.UserId, StringComparison.Ordinal) &&
            !string.Equals(bid.Status, BidStatuses.Rejected, StringComparison.Ordinal));

        if (hasExistingActiveBid)
        {
            throw new ArgumentException("Vec imate aktivnu ponudu za ovaj posao.");
        }

        var hasAcceptedBid = jobBids.Any(bid => string.Equals(bid.Status, BidStatuses.Accepted, StringComparison.Ordinal));
        if (hasAcceptedBid)
        {
            throw new ArgumentException("Za ovaj posao je vec odabrana ponuda.");
        }

        var bid = new Bid(
            Guid.NewGuid().ToString("N"),
            normalizedJobId,
            userContext.UserId,
            userContext.DisplayName?.Trim() is { Length: > 0 } displayName ? displayName : userContext.UserId,
            request.Amount,
            request.DaysToComplete,
            request.Message.Trim(),
            BidStatuses.Pending,
            DateTimeOffset.UtcNow);

        var createdBid = _bidRepository.Create(bid);
        return MapToResponse(createdBid);
    }

    public IReadOnlyCollection<BidResponse> AcceptBid(string bidId, RequestUserContext userContext)
    {
        ValidateAcceptContext(userContext);

        var bid = _bidRepository.GetById(bidId.Trim());
        if (bid is null)
        {
            throw new ArgumentException("Ponuda nije pronadjena.");
        }

        var job = _jobRepository.GetById(bid.JobId);
        if (job is null)
        {
            throw new ArgumentException("Posao nije pronadjen.");
        }

        if (!string.Equals(job.InvestitorId, userContext.UserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Ne mozete prihvatiti ponudu za posao koji nije vas.");
        }

        var updatedJobBids = _bidRepository
            .GetByJobId(bid.JobId)
            .Select(existingBid =>
            {
                if (string.Equals(existingBid.Id, bid.Id, StringComparison.Ordinal))
                {
                    return existingBid with { Status = BidStatuses.Accepted };
                }

                return existingBid with { Status = BidStatuses.Rejected };
            })
            .ToArray();

        _bidRepository.ReplaceForJob(bid.JobId, updatedJobBids);

        return updatedJobBids
            .OrderByDescending(existingBid => existingBid.CreatedAt)
            .Select(MapToResponse)
            .ToArray();
    }

    private static void ValidateCreateContext(RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        if (!string.Equals(userContext.Role, BuildConnectRoles.Izvodjac, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo izvodjaci mogu slati ponude.");
        }
    }

    private static void ValidateAcceptContext(RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        if (!string.Equals(userContext.Role, BuildConnectRoles.Investitor, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo investitori mogu prihvatiti ponude.");
        }
    }

    private static void ValidateCreateRequest(CreateBidRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Iznos ponude mora biti veci od 0.");
        }

        if (request.DaysToComplete <= 0)
        {
            throw new ArgumentException("Rok izvedbe mora biti veci od 0 dana.");
        }

        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length < 10)
        {
            throw new ArgumentException("Poruka ponude mora imati barem 10 znakova.");
        }
    }

    private static BidResponse MapToResponse(Bid bid)
    {
        return new BidResponse(
            bid.Id,
            bid.JobId,
            bid.ContractorId,
            bid.ContractorName,
            bid.Amount,
            bid.DaysToComplete,
            bid.Message,
            bid.Status,
            bid.CreatedAt);
    }
}
