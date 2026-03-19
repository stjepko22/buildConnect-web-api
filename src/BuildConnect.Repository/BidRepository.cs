using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;

namespace BuildConnect.Repository;

public sealed class BidRepository : IBidRepository
{
    private readonly InMemoryBidDataStore _bidDataStore;

    public BidRepository(InMemoryBidDataStore bidDataStore)
    {
        _bidDataStore = bidDataStore;
    }

    public IReadOnlyCollection<Bid> GetAll()
    {
        return _bidDataStore.GetAll();
    }

    public IReadOnlyCollection<Bid> GetByJobId(string jobId)
    {
        return _bidDataStore.GetByJobId(jobId);
    }

    public IReadOnlyCollection<Bid> GetByContractorId(string contractorId)
    {
        return _bidDataStore.GetByContractorId(contractorId);
    }

    public Bid? GetById(string id)
    {
        return _bidDataStore.GetById(id);
    }

    public Bid Create(Bid bid)
    {
        return _bidDataStore.Add(bid);
    }

    public void ReplaceForJob(string jobId, IReadOnlyCollection<Bid> bids)
    {
        _bidDataStore.ReplaceForJob(jobId, bids);
    }
}
