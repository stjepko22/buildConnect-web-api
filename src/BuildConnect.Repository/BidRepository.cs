using BuildConnect.DAL;
using BuildConnect.DAL.Entities;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.Repository;

public sealed class BidRepository : IBidRepository
{
    private readonly BuildConnectDbContext _dbContext;

    public BidRepository(BuildConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<Bid> GetAll()
    {
        return _dbContext.Bids
            .AsNoTracking()
            .OrderByDescending(bid => bid.CreatedAt)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public IReadOnlyCollection<Bid> GetByJobId(string jobId)
    {
        return _dbContext.Bids
            .AsNoTracking()
            .Where(bid => bid.JobId == jobId)
            .OrderByDescending(bid => bid.CreatedAt)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public IReadOnlyCollection<Bid> GetByContractorId(string contractorId)
    {
        return _dbContext.Bids
            .AsNoTracking()
            .Where(bid => bid.ContractorId == contractorId)
            .OrderByDescending(bid => bid.CreatedAt)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public Bid? GetById(string id)
    {
        var bid = _dbContext.Bids
            .AsNoTracking()
            .FirstOrDefault(bid => bid.Id == id);

        return bid is null ? null : MapToModel(bid);
    }

    public Bid Create(Bid bid)
    {
        var entity = new BidEntity
        {
            Id = bid.Id,
            JobId = bid.JobId,
            ContractorId = bid.ContractorId,
            ContractorName = bid.ContractorName,
            Amount = bid.Amount,
            DaysToComplete = bid.DaysToComplete,
            Message = bid.Message,
            Status = bid.Status,
            CreatedAt = bid.CreatedAt
        };

        _dbContext.Bids.Add(entity);
        _dbContext.SaveChanges();

        return MapToModel(entity);
    }

    public void ReplaceForJob(string jobId, IReadOnlyCollection<Bid> bids)
    {
        var existingBids = _dbContext.Bids
            .Where(bid => bid.JobId == jobId)
            .ToArray();

        var bidsById = bids.ToDictionary(bid => bid.Id, StringComparer.Ordinal);

        foreach (var existingBid in existingBids)
        {
            if (!bidsById.TryGetValue(existingBid.Id, out var updatedBid))
            {
                continue;
            }

            existingBid.ContractorName = updatedBid.ContractorName;
            existingBid.Amount = updatedBid.Amount;
            existingBid.DaysToComplete = updatedBid.DaysToComplete;
            existingBid.Message = updatedBid.Message;
            existingBid.Status = updatedBid.Status;
            existingBid.CreatedAt = updatedBid.CreatedAt;
        }

        _dbContext.SaveChanges();
    }

    private static Bid MapToModel(BidEntity bid)
    {
        return new Bid(
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
