using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IDashboardService
{
    DashboardResponse GetDashboard(RequestUserContext userContext);
}
