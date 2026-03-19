using BuildConnect.Model;

namespace BuildConnect.DAL;

public sealed class InMemoryBidDataStore
{
    private readonly object _syncRoot = new();
    private readonly List<Bid> _bids = [];

    public IReadOnlyCollection<Bid> GetAll()
    {
        lock (_syncRoot)
        {
            return _bids
                .OrderByDescending(bid => bid.CreatedAt)
                .ToArray();
        }
    }

    public IReadOnlyCollection<Bid> GetByJobId(string jobId)
    {
        lock (_syncRoot)
        {
            return _bids
                .Where(bid => string.Equals(bid.JobId, jobId, StringComparison.Ordinal))
                .OrderByDescending(bid => bid.CreatedAt)
                .ToArray();
        }
    }

    public IReadOnlyCollection<Bid> GetByContractorId(string contractorId)
    {
        lock (_syncRoot)
        {
            return _bids
                .Where(bid => string.Equals(bid.ContractorId, contractorId, StringComparison.Ordinal))
                .OrderByDescending(bid => bid.CreatedAt)
                .ToArray();
        }
    }

    public Bid? GetById(string id)
    {
        lock (_syncRoot)
        {
            return _bids.FirstOrDefault(bid => string.Equals(bid.Id, id, StringComparison.Ordinal));
        }
    }

    public Bid Add(Bid bid)
    {
        lock (_syncRoot)
        {
            _bids.Insert(0, bid);
            return bid;
        }
    }

    public void ReplaceForJob(string jobId, IReadOnlyCollection<Bid> bids)
    {
        lock (_syncRoot)
        {
            _bids.RemoveAll(existingBid => string.Equals(existingBid.JobId, jobId, StringComparison.Ordinal));
            _bids.AddRange(bids);
        }
    }
}
