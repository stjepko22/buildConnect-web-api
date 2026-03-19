using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;

namespace BuildConnect.Repository;

public sealed class JobRepository : IJobRepository
{
    private readonly InMemoryJobDataStore _jobDataStore;

    public JobRepository(InMemoryJobDataStore jobDataStore)
    {
        _jobDataStore = jobDataStore;
    }

    public IReadOnlyCollection<Job> GetAll()
    {
        return _jobDataStore.GetAll();
    }

    public Job? GetById(string id)
    {
        return _jobDataStore.GetById(id);
    }

    public Job Create(Job job)
    {
        return _jobDataStore.Add(job);
    }
}
