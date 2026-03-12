using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IApiStatusRepository
{
    ApiMetadata GetApiMetadata();
}
