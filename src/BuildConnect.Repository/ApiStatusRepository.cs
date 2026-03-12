using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;

namespace BuildConnect.Repository;

public sealed class ApiStatusRepository : IApiStatusRepository
{
    private readonly ApplicationRuntimeInfoProvider _runtimeInfoProvider;

    public ApiStatusRepository(ApplicationRuntimeInfoProvider runtimeInfoProvider)
    {
        _runtimeInfoProvider = runtimeInfoProvider;
    }

    public ApiMetadata GetApiMetadata()
    {
        return _runtimeInfoProvider.GetMetadata();
    }
}
