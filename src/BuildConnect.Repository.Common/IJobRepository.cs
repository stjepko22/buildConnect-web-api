using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IJobRepository
{
    IReadOnlyCollection<Job> GetAll();

    Job? GetById(string id);

    Job Create(Job job);
}
