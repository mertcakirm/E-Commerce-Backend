using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface ICommentService
{
    Task<Comment?> GetByIdAsync(int id);
    Task<ServiceResult<PagedResult<CommentListDto>>> GetCommentsByProductIdAsync(int productId, int pageNumber, int pageSize);
    Task<ServiceResult<Comment?>>  AddCommentAsync(CommentCreateDto commentDto, string token);
    Task<ServiceResult<bool>> DeleteCommentAsync(int id, string token);
}