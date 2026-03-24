using BuildConnect.DAL;
using BuildConnect.DAL.Entities;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.Repository;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly BuildConnectDbContext _dbContext;

    public ReviewRepository(BuildConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<Review> GetAll()
    {
        return _dbContext.Reviews
            .AsNoTracking()
            .OrderByDescending(review => review.CreatedAt)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public IReadOnlyCollection<Review> GetByRevieweeId(string revieweeId)
    {
        return _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.RevieweeId == revieweeId)
            .OrderByDescending(review => review.CreatedAt)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public Review? GetByJobId(string jobId)
    {
        var review = _dbContext.Reviews
            .AsNoTracking()
            .FirstOrDefault(review => review.JobId == jobId);

        return review is null ? null : MapToModel(review);
    }

    public Review Create(Review review)
    {
        var entity = new ReviewEntity
        {
            Id = review.Id,
            JobId = review.JobId,
            ReviewerId = review.ReviewerId,
            RevieweeId = review.RevieweeId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        _dbContext.Reviews.Add(entity);
        _dbContext.SaveChanges();

        return MapToModel(entity);
    }

    private static Review MapToModel(ReviewEntity review)
    {
        return new Review(
            review.Id,
            review.JobId,
            review.ReviewerId,
            review.RevieweeId,
            review.Rating,
            review.Comment,
            review.CreatedAt);
    }
}
