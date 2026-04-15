using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IJobService
{
    IReadOnlyCollection<JobResponse> GetJobs();

    JobResponse? GetJobById(string id);

    JobResponse CreateJob(CreateJobRequest request, RequestUserContext userContext);

    JobResponse? UpdateJob(string id, UpdateJobRequest request, RequestUserContext userContext);
}
