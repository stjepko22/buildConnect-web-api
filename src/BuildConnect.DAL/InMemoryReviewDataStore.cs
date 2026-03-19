using BuildConnect.Model;

namespace BuildConnect.DAL;

public sealed class InMemoryReviewDataStore
{
    private readonly object _syncRoot = new();
    private readonly List<Review> _reviews =
    [
        new(
            "review-1",
            "posao-1",
            "investitor-1",
            "izvodjac-1",
            5,
            "Odlicna izvedba fasade, sve je zavrseno u roku.",
            new DateTimeOffset(2026, 2, 12, 0, 0, 0, TimeSpan.Zero)),
        new(
            "review-2",
            "posao-2",
            "investitor-2",
            "izvodjac-2",
            4,
            "Kvalitetan posao i dobra komunikacija kroz cijeli projekt.",
            new DateTimeOffset(2026, 2, 13, 0, 0, 0, TimeSpan.Zero)),
        new(
            "review-3",
            "posao-4",
            "investitor-2",
            "izvodjac-3",
            5,
            "Profesionalan elektro tim, preporuka za poslovne objekte.",
            new DateTimeOffset(2026, 2, 15, 0, 0, 0, TimeSpan.Zero))
    ];

    public IReadOnlyCollection<Review> GetAll()
    {
        lock (_syncRoot)
        {
            return _reviews
                .OrderByDescending(review => review.CreatedAt)
                .ToArray();
        }
    }

    public IReadOnlyCollection<Review> GetByRevieweeId(string revieweeId)
    {
        lock (_syncRoot)
        {
            return _reviews
                .Where(review => string.Equals(review.RevieweeId, revieweeId, StringComparison.Ordinal))
                .OrderByDescending(review => review.CreatedAt)
                .ToArray();
        }
    }

    public Review? GetByJobId(string jobId)
    {
        lock (_syncRoot)
        {
            return _reviews.FirstOrDefault(review => string.Equals(review.JobId, jobId, StringComparison.Ordinal));
        }
    }

    public Review Add(Review review)
    {
        lock (_syncRoot)
        {
            _reviews.Insert(0, review);
            return review;
        }
    }
}
