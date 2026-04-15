using BuildConnect.DAL;
using BuildConnect.DAL.Entities;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.Repository;

public sealed class JobRepository : IJobRepository
{
    private readonly BuildConnectDbContext _dbContext;

    public JobRepository(BuildConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<Job> GetAll()
    {
        return _dbContext.Jobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public Job? GetById(string id)
    {
        var job = _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefault(job => job.Id == id);

        return job is null ? null : MapToModel(job);
    }

    public Job Create(Job job)
    {
        var entity = new JobEntity
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Category = job.Category,
            Location = job.Location,
            Budget = job.Budget,
            Deadline = job.Deadline,
            InvestitorId = job.InvestitorId,
            CreatedAt = job.CreatedAt
        };

        _dbContext.Jobs.Add(entity);
        _dbContext.SaveChanges();

        return MapToModel(entity);
    }

    public Job? Update(Job job)
    {
        var entity = _dbContext.Jobs.FirstOrDefault(existingJob => existingJob.Id == job.Id);
        if (entity is null)
        {
            return null;
        }

        entity.Title = job.Title;
        entity.Description = job.Description;
        entity.Category = job.Category;
        entity.Location = job.Location;
        entity.Budget = job.Budget;
        entity.Deadline = job.Deadline;

        _dbContext.SaveChanges();
        return MapToModel(entity);
    }

    private static Job MapToModel(JobEntity job)
    {
        return new Job(
            job.Id,
            job.Title,
            job.Description,
            job.Category,
            job.Location,
            job.Budget,
            job.Deadline,
            job.InvestitorId,
            job.CreatedAt);
    }
}
