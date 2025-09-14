using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId);
    Task<double> GetAverageRatingByProductIdAsync(int productId);
    Task AddCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);
    
}