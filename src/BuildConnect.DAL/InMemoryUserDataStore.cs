using BuildConnect.Model;

namespace BuildConnect.DAL;

public sealed class InMemoryUserDataStore
{
    private readonly object _syncRoot = new();
    private readonly List<UserProfile> _users =
    [
        new(
            "investitor-1",
            "Marko Markovic",
            BuildConnectRoles.Investitor,
            BuildConnectLegalTypes.Firma,
            "investitor@buildconnect.hr",
            "Trazim pouzdane izvodace za projekte renovacije stanova u Zagrebu.",
            "Zagreb",
            new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)),
        new(
            "investitor-2",
            "Ana Anic",
            BuildConnectRoles.Investitor,
            BuildConnectLegalTypes.Firma,
            "ana@test.com",
            "Investitor s fokusom na moderne niskoenergetske kuce.",
            "Split",
            new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero)),
        new(
            "izvodjac-1",
            "Ivan Ivic - Gradnja d.o.o.",
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.Firma,
            "izvodjac@buildconnect.hr",
            "Specijalizirani za fasaderske radove i suhu gradnju. 15 godina iskustva.",
            "Zagreb",
            new DateTimeOffset(2024, 11, 20, 0, 0, 0, TimeSpan.Zero),
            ["Fasade", "Gradnja", "Renovacija"]),
        new(
            "izvodjac-2",
            "Petar Horvat",
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.FizickaOsoba,
            "petar@majstor.hr",
            "Samostalni keramicar s fokusom na kupaonice i kuhinje.",
            "Split",
            new DateTimeOffset(2024, 10, 12, 0, 0, 0, TimeSpan.Zero),
            ["Keramika", "Renovacija"]),
        new(
            "izvodjac-3",
            "Elektro Napon d.o.o.",
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.Firma,
            "info@napon.hr",
            "Elektro tim za stambene i poslovne objekte.",
            "Zadar",
            new DateTimeOffset(2024, 9, 3, 0, 0, 0, TimeSpan.Zero),
            ["Elektro"]),
        new(
            "izvodjac-4",
            "Krov Plus Obrt",
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.Firma,
            "kontakt@krovplus.hr",
            "Krovopokrivacki i limarski radovi na novogradnji i adaptacijama.",
            "Rijeka",
            new DateTimeOffset(2024, 8, 19, 0, 0, 0, TimeSpan.Zero),
            ["Krovovi", "Stolarija"]),
        new(
            "izvodjac-5",
            "Nikola Vukovic",
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.FizickaOsoba,
            "nikola@vodomajstor.hr",
            "Vodoinstalater i monter sustava grijanja.",
            "Osijek",
            new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero),
            ["Vodoinstalacije", "Grijanje"])
    ];

    public IReadOnlyCollection<UserProfile> GetAll()
    {
        lock (_syncRoot)
        {
            return _users
                .OrderBy(user => user.DisplayName, StringComparer.Ordinal)
                .ToArray();
        }
    }

    public UserProfile? GetById(string id)
    {
        lock (_syncRoot)
        {
            return _users.FirstOrDefault(user => string.Equals(user.Id, id, StringComparison.Ordinal));
        }
    }

    public UserProfile? GetByEmail(string email)
    {
        lock (_syncRoot)
        {
            return _users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyCollection<UserProfile> GetByRole(string role)
    {
        lock (_syncRoot)
        {
            return _users
                .Where(user => string.Equals(user.Role, role, StringComparison.Ordinal))
                .OrderBy(user => user.DisplayName, StringComparer.Ordinal)
                .ToArray();
        }
    }

    public UserProfile Add(UserProfile user)
    {
        lock (_syncRoot)
        {
            _users.Add(user);
            return user;
        }
    }
}
