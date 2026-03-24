using BuildConnect.Model;
using Microsoft.AspNetCore.Identity;

namespace BuildConnect.Tests.Support;

internal static class TestData
{
    public static UserProfile CreateInvestitor(
        string id = "investitor-1",
        string email = "investitor@test.hr",
        string displayName = "Marko Markovic",
        string passwordHash = "")
    {
        return new UserProfile(
            id,
            displayName,
            BuildConnectRoles.Investitor,
            BuildConnectLegalTypes.Firma,
            email,
            "Bio",
            "Zagreb",
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            null,
            passwordHash);
    }

    public static UserProfile CreateIzvodjac(
        string id = "izvodjac-1",
        string email = "izvodjac@test.hr",
        string displayName = "Ivan Ivic",
        string passwordHash = "")
    {
        return new UserProfile(
            id,
            displayName,
            BuildConnectRoles.Izvodjac,
            BuildConnectLegalTypes.Firma,
            email,
            "Bio",
            "Split",
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ["Gradnja"],
            passwordHash);
    }

    public static Job CreateJob(
        string id = "posao-1",
        string investitorId = "investitor-1")
    {
        return new Job(
            id,
            "Izrada fasade",
            "Potrebna izrada fasade na obiteljskoj kuci.",
            "Fasade",
            "Zagreb",
            3500m,
            "2026-05-01",
            investitorId,
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));
    }

    public static Bid CreateBid(
        string id = "bid-1",
        string jobId = "posao-1",
        string contractorId = "izvodjac-1",
        string status = BidStatuses.Pending)
    {
        return new Bid(
            id,
            jobId,
            contractorId,
            "Ivan Ivic",
            3200m,
            14,
            "Mozemo krenuti odmah i zavrsiti brzo.",
            status,
            new DateTimeOffset(2026, 2, 2, 0, 0, 0, TimeSpan.Zero));
    }

    public static Review CreateReview(
        string id = "review-1",
        string jobId = "posao-1",
        string reviewerId = "investitor-1",
        string revieweeId = "izvodjac-1")
    {
        return new Review(
            id,
            jobId,
            reviewerId,
            revieweeId,
            5,
            "Vrlo kvalitetno odradjen posao i dobra komunikacija.",
            new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero));
    }

    public static (UserProfile user, AuthAccount account, string plainPassword) CreateAuthUser(
        string role,
        string email,
        string plainPassword)
    {
        var passwordHasher = new PasswordHasher<AuthAccount>();
        var user = role == BuildConnectRoles.Investitor
            ? CreateInvestitor(email: email)
            : CreateIzvodjac(email: email);
        var account = new AuthAccount(user.Id, email, string.Empty);
        var passwordHash = passwordHasher.HashPassword(account, plainPassword);

        var userWithHash = user with { PasswordHash = passwordHash };
        var accountWithHash = account with { PasswordHash = passwordHash };

        return (userWithHash, accountWithHash, plainPassword);
    }
}
