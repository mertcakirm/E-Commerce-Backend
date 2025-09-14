using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface ICommentService
{
    Task<Comment?> GetByIdAsync(int id);
    Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId);
    Task<double> GetAverageRatingByProductIdAsync(int productId);
    Task<Comment?> AddCommentAsync(CommentCreateDto commentDto, string token);
    Task<bool> DeleteCommentAsync(int id, string token);
}