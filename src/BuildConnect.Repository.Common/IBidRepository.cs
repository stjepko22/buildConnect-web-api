using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IBidRepository
{
    IReadOnlyCollection<Bid> GetAll();

    IReadOnlyCollection<Bid> GetByJobId(string jobId);

    IReadOnlyCollection<Bid> GetByContractorId(string contractorId);

    Bid? GetById(string id);

    Bid Create(Bid bid);

    void ReplaceForJob(string jobId, IReadOnlyCollection<Bid> bids);
}
