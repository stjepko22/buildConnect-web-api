using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class ApiStatusService : IApiStatusService
{
    private readonly IApiStatusRepository _apiStatusRepository;

    public ApiStatusService(IApiStatusRepository apiStatusRepository)
    {
        _apiStatusRepository = apiStatusRepository;
    }

    public ApiStatus GetStatus()
    {
        var metadata = _apiStatusRepository.GetApiMetadata();

        return new ApiStatus(
            metadata.Name,
            metadata.Environment,
            metadata.Version,
            DateTimeOffset.UtcNow);
    }
}
