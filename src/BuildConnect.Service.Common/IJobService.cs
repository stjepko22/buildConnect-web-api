using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IJobService
{
    IReadOnlyCollection<JobResponse> GetJobs(GetJobsRequest? request = null);

    JobResponse? GetJobById(string id);

    JobResponse CreateJob(CreateJobRequest request, RequestUserContext userContext);

    JobResponse? UpdateJob(string id, UpdateJobRequest request, RequestUserContext userContext);

    JobResponse? CloseJob(string id, RequestUserContext userContext);

    JobResponse? CompleteJob(string id, RequestUserContext userContext);

    bool DeleteJob(string id, RequestUserContext userContext);
}
