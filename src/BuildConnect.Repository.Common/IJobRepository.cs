using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IJobRepository
{
    IReadOnlyCollection<Job> GetAll(GetJobsRequest? request = null);

    Job? GetById(string id);

    Job Create(Job job);

    Job? Update(Job job);

    Job? UpdateStatus(string id, string status);

    bool Delete(string id);
}
