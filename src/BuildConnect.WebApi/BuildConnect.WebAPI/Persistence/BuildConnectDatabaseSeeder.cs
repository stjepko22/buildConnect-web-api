using System.Text.Json;
using BuildConnect.DAL;
using BuildConnect.DAL.Entities;
using BuildConnect.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.WebAPI.Persistence;

public sealed class BuildConnectDatabaseSeeder
{
    private readonly BuildConnectDbContext _dbContext;
    private readonly IPasswordHasher<AuthAccount> _passwordHasher;

    public BuildConnectDatabaseSeeder(BuildConnectDbContext dbContext, IPasswordHasher<AuthAccount> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task MigrateAndSeedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        await SeedUsersAsync(cancellationToken);
        await SeedJobsAsync(cancellationToken);
        await SeedBidsAsync(cancellationToken);
        await SeedReviewsAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var users = new[]
        {
            CreateUser("investitor-1", "investitor@buildconnect.hr", "Marko Markovic", BuildConnectRoles.Investitor, BuildConnectLegalTypes.Firma, "Trazim pouzdane izvodace za projekte renovacije stanova u Zagrebu.", "Zagreb", new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero), null, "invest123"),
            CreateUser("investitor-2", "ana@test.com", "Ana Anic", BuildConnectRoles.Investitor, BuildConnectLegalTypes.Firma, "Investitor s fokusom na moderne niskoenergetske kuce.", "Split", new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero), null, "buildconnect-demo-123"),
            CreateUser("izvodjac-1", "izvodjac@buildconnect.hr", "Ivan Ivic - Gradnja d.o.o.", BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma, "Specijalizirani za fasaderske radove i suhu gradnju. 15 godina iskustva.", "Zagreb", new DateTimeOffset(2024, 11, 20, 0, 0, 0, TimeSpan.Zero), ["Fasade", "Gradnja", "Renovacija"], "izvodjac123"),
            CreateUser("izvodjac-2", "petar@majstor.hr", "Petar Horvat", BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.FizickaOsoba, "Samostalni keramicar s fokusom na kupaonice i kuhinje.", "Split", new DateTimeOffset(2024, 10, 12, 0, 0, 0, TimeSpan.Zero), ["Keramika", "Renovacija"], "buildconnect-demo-123"),
            CreateUser("izvodjac-3", "info@napon.hr", "Elektro Napon d.o.o.", BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma, "Elektro tim za stambene i poslovne objekte.", "Zadar", new DateTimeOffset(2024, 9, 3, 0, 0, 0, TimeSpan.Zero), ["Elektro"], "buildconnect-demo-123"),
            CreateUser("izvodjac-4", "kontakt@krovplus.hr", "Krov Plus Obrt", BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma, "Krovopokrivacki i limarski radovi na novogradnji i adaptacijama.", "Rijeka", new DateTimeOffset(2024, 8, 19, 0, 0, 0, TimeSpan.Zero), ["Krovovi", "Stolarija"], "buildconnect-demo-123"),
            CreateUser("izvodjac-5", "nikola@vodomajstor.hr", "Nikola Vukovic", BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.FizickaOsoba, "Vodoinstalater i monter sustava grijanja.", "Osijek", new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero), ["Vodoinstalacije", "Grijanje"], "buildconnect-demo-123")
        };

        await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedJobsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Jobs.AnyAsync(cancellationToken))
        {
            return;
        }

        var jobs = new[]
        {
            new JobEntity { Id = "posao-1", Title = "Izrada fasade na obiteljskoj kuci", Description = "Potrebna izrada termo fasade (stiropor 10cm) na objektu od 200m2. Materijal osiguran.", Category = "Fasade", Location = "Zagreb", Budget = 3500m, Deadline = "2026-05-01", InvestitorId = "investitor-1", CreatedAt = new DateTimeOffset(2026, 2, 8, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-2", Title = "Postavljanje keramike u kupaonici", Description = "Potrebno postaviti 40m2 plocica u novogradnji. Podloga je spremna.", Category = "Keramika", Location = "Split", Budget = 800m, Deadline = "2026-03-15", InvestitorId = "investitor-2", CreatedAt = new DateTimeOffset(2026, 2, 9, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-3", Title = "Sanacija krova na poslovnom objektu", Description = "Potrebna zamjena dotrajale limarije i hidroizolacije na krovu povrsine 320m2.", Category = "Krovovi", Location = "Rijeka", Budget = 6200m, Deadline = "2026-06-10", InvestitorId = "investitor-1", CreatedAt = new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-4", Title = "Kompletna elektro instalacija stana", Description = "Novogradnja 85m2. Razvod ormara, uticnice, rasvjeta i priprema za pametni sustav.", Category = "Elektro", Location = "Zadar", Budget = 2800m, Deadline = "2026-04-20", InvestitorId = "investitor-2", CreatedAt = new DateTimeOffset(2026, 2, 11, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-5", Title = "Vodoinstalaterski radovi u kuci", Description = "Potrebna zamjena glavnih cijevi i ugradnja novih prikljucaka u dvije kupaonice.", Category = "Vodoinstalacije", Location = "Osijek", Budget = 1900m, Deadline = "2026-04-02", InvestitorId = "investitor-1", CreatedAt = new DateTimeOffset(2026, 2, 13, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-6", Title = "Ugradnja podnog grijanja", Description = "Projekt obuhvaca 110m2 prostora, pripremu podloge i test sustava prije glazure.", Category = "Grijanje", Location = "Varazdin", Budget = 3400m, Deadline = "2026-05-18", InvestitorId = "investitor-2", CreatedAt = new DateTimeOffset(2026, 2, 14, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-7", Title = "Renovacija ureda open-space", Description = "Rusenje pregradnih zidova, gletanje, bojanje i priprema instalacija za nove pozicije.", Category = "Renovacija", Location = "Zagreb", Budget = 7600m, Deadline = "2026-05-30", InvestitorId = "investitor-1", CreatedAt = new DateTimeOffset(2026, 2, 16, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-8", Title = "Izrada drvene vanjske stolarije", Description = "Potrebna izrada i montaza 6 prozora i 2 balkonska vrata od lameliranog drveta.", Category = "Stolarija", Location = "Pula", Budget = 5100m, Deadline = "2026-06-25", InvestitorId = "investitor-2", CreatedAt = new DateTimeOffset(2026, 2, 18, 0, 0, 0, TimeSpan.Zero) },
            new JobEntity { Id = "posao-9", Title = "Priprema gradilista i grubi gradevinski radovi", Description = "Potrebna ekipa za iskope, oplatu i betoniranje temeljne ploce za obiteljsku kucu.", Category = "Gradnja", Location = "Sisak", Budget = 12400m, Deadline = "2026-07-05", InvestitorId = "investitor-1", CreatedAt = new DateTimeOffset(2026, 2, 20, 0, 0, 0, TimeSpan.Zero) }
        };

        await _dbContext.Jobs.AddRangeAsync(jobs, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedBidsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Bids.AnyAsync(cancellationToken))
        {
            return;
        }

        var bids = new[]
        {
            new BidEntity { Id = "bid-1", JobId = "posao-1", ContractorId = "izvodjac-1", ContractorName = "Ivan Ivic - Gradnja d.o.o.", Amount = 3300m, DaysToComplete = 21, Message = "Mozemo krenuti odmah i zavrsiti unutar tri tjedna.", Status = BidStatuses.Accepted, CreatedAt = new DateTimeOffset(2026, 2, 8, 12, 0, 0, TimeSpan.Zero) },
            new BidEntity { Id = "bid-2", JobId = "posao-2", ContractorId = "izvodjac-2", ContractorName = "Petar Horvat", Amount = 780m, DaysToComplete = 5, Message = "Imam iskustva s kupaonicama i mogu poceti ovaj tjedan.", Status = BidStatuses.Accepted, CreatedAt = new DateTimeOffset(2026, 2, 9, 12, 0, 0, TimeSpan.Zero) },
            new BidEntity { Id = "bid-3", JobId = "posao-4", ContractorId = "izvodjac-3", ContractorName = "Elektro Napon d.o.o.", Amount = 2950m, DaysToComplete = 14, Message = "Tim je slobodan iduci ponedjeljak i imamo iskustva s pametnim sustavima.", Status = BidStatuses.Accepted, CreatedAt = new DateTimeOffset(2026, 2, 11, 14, 0, 0, TimeSpan.Zero) },
            new BidEntity { Id = "bid-4", JobId = "posao-3", ContractorId = "izvodjac-4", ContractorName = "Krov Plus Obrt", Amount = 6100m, DaysToComplete = 18, Message = "Specijalizirani smo za ovu vrstu krovova i mozemo isporuciti kompletan posao.", Status = BidStatuses.Pending, CreatedAt = new DateTimeOffset(2026, 2, 12, 10, 0, 0, TimeSpan.Zero) },
            new BidEntity { Id = "bid-5", JobId = "posao-5", ContractorId = "izvodjac-5", ContractorName = "Nikola Vukovic", Amount = 1850m, DaysToComplete = 7, Message = "Mogu odraditi kompletne vodoinstalaterske radove i testiranje sustava.", Status = BidStatuses.Pending, CreatedAt = new DateTimeOffset(2026, 2, 13, 15, 0, 0, TimeSpan.Zero) }
        };

        await _dbContext.Bids.AddRangeAsync(bids, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedReviewsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Reviews.AnyAsync(cancellationToken))
        {
            return;
        }

        var reviews = new[]
        {
            new ReviewEntity { Id = "review-1", JobId = "posao-1", ReviewerId = "investitor-1", RevieweeId = "izvodjac-1", Rating = 5, Comment = "Vrlo profesionalna izvedba, sve odradeno u dogovorenom roku i uz odlicnu komunikaciju.", CreatedAt = new DateTimeOffset(2026, 2, 28, 0, 0, 0, TimeSpan.Zero) },
            new ReviewEntity { Id = "review-2", JobId = "posao-2", ReviewerId = "investitor-2", RevieweeId = "izvodjac-2", Rating = 4, Comment = "Kvalitetno izvedeni keramicki radovi i korektan odnos tijekom cijelog projekta.", CreatedAt = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero) },
            new ReviewEntity { Id = "review-3", JobId = "posao-4", ReviewerId = "investitor-2", RevieweeId = "izvodjac-3", Rating = 5, Comment = "Odlican elektro tim, uredni, tocni i vrlo dobro organizirani na gradilistu.", CreatedAt = new DateTimeOffset(2026, 3, 6, 0, 0, 0, TimeSpan.Zero) }
        };

        await _dbContext.Reviews.AddRangeAsync(reviews, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private UserEntity CreateUser(
        string id,
        string email,
        string displayName,
        string role,
        string legalType,
        string bio,
        string location,
        DateTimeOffset joinedAt,
        IReadOnlyCollection<string>? serviceCategories,
        string password)
    {
        var user = new UserEntity
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
            Role = role,
            LegalType = legalType,
            Bio = bio,
            Location = location,
            JoinedAt = joinedAt,
            ServiceCategoriesJson = serviceCategories is null ? null : JsonSerializer.Serialize(serviceCategories)
        };

        var account = new AuthAccount(id, email, string.Empty);
        user.PasswordHash = _passwordHasher.HashPassword(account, password);
        return user;
    }
}
