using BuildConnect.Model;

namespace BuildConnect.DAL;

public sealed class InMemoryJobDataStore
{
    private readonly object _syncRoot = new();
    private readonly List<Job> _jobs =
    [
        new(
            "posao-1",
            "Izrada fasade na obiteljskoj kuci",
            "Potrebna izrada termo fasade (stiropor 10cm) na objektu od 200m2. Materijal osiguran.",
            "Fasade",
            "Zagreb",
            3500m,
            "2026-05-01",
            "investitor-1",
            new DateTimeOffset(2026, 2, 8, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-2",
            "Postavljanje keramike u kupaonici",
            "Potrebno postaviti 40m2 plocica u novogradnji. Podloga je spremna.",
            "Keramika",
            "Split",
            800m,
            "2026-03-15",
            "investitor-2",
            new DateTimeOffset(2026, 2, 9, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-3",
            "Sanacija krova na poslovnom objektu",
            "Potrebna zamjena dotrajale limarije i hidroizolacije na krovu povrsine 320m2.",
            "Krovovi",
            "Rijeka",
            6200m,
            "2026-06-10",
            "investitor-1",
            new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-4",
            "Kompletna elektro instalacija stana",
            "Novogradnja 85m2. Razvod ormara, uticnice, rasvjeta i priprema za pametni sustav.",
            "Elektro",
            "Zadar",
            2800m,
            "2026-04-20",
            "investitor-2",
            new DateTimeOffset(2026, 2, 11, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-5",
            "Vodoinstalaterski radovi u kuci",
            "Potrebna zamjena glavnih cijevi i ugradnja novih prikljucaka u dvije kupaonice.",
            "Vodoinstalacije",
            "Osijek",
            1900m,
            "2026-04-02",
            "investitor-1",
            new DateTimeOffset(2026, 2, 13, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-6",
            "Ugradnja podnog grijanja",
            "Projekt obuhvaca 110m2 prostora, pripremu podloge i test sustava prije glazure.",
            "Grijanje",
            "Varazdin",
            3400m,
            "2026-05-18",
            "investitor-2",
            new DateTimeOffset(2026, 2, 14, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-7",
            "Renovacija ureda open-space",
            "Rusenje pregradnih zidova, gletanje, bojanje i priprema instalacija za nove pozicije.",
            "Renovacija",
            "Zagreb",
            7600m,
            "2026-05-30",
            "investitor-1",
            new DateTimeOffset(2026, 2, 16, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-8",
            "Izrada drvene vanjske stolarije",
            "Potrebna izrada i montaza 6 prozora i 2 balkonska vrata od lameliranog drveta.",
            "Stolarija",
            "Pula",
            5100m,
            "2026-06-25",
            "investitor-2",
            new DateTimeOffset(2026, 2, 18, 0, 0, 0, TimeSpan.Zero)),
        new(
            "posao-9",
            "Priprema gradilista i grubi gradevinski radovi",
            "Potrebna ekipa za iskope, oplatu i betoniranje temeljne ploce za obiteljsku kucu.",
            "Gradnja",
            "Sisak",
            12400m,
            "2026-07-05",
            "investitor-1",
            new DateTimeOffset(2026, 2, 20, 0, 0, 0, TimeSpan.Zero))
    ];

    public IReadOnlyCollection<Job> GetAll()
    {
        lock (_syncRoot)
        {
            return _jobs
                .OrderByDescending(job => job.CreatedAt)
                .ToArray();
        }
    }

    public Job? GetById(string id)
    {
        lock (_syncRoot)
        {
            return _jobs.FirstOrDefault(job => string.Equals(job.Id, id, StringComparison.Ordinal));
        }
    }

    public Job Add(Job job)
    {
        lock (_syncRoot)
        {
            _jobs.Insert(0, job);
            return job;
        }
    }
}
