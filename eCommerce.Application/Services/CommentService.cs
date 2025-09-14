using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITokenService _tokenService;

        public CommentService(ICommentRepository commentRepository, ITokenService tokenService)
        {
            _commentRepository = commentRepository;
            _tokenService = tokenService;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId)
        {
            return await _commentRepository.GetCommentsByProductIdAsync(productId);
        }

        public async Task<double> GetAverageRatingByProductIdAsync(int productId)
        {
            return await _commentRepository.GetAverageRatingByProductIdAsync(productId);
        }

        public async Task<Comment?> AddCommentAsync(CommentCreateDto commentDto, string token)
        {
            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null) return null;

            var newComment = new Comment
            {
                UserId = userId,
                ProductId = commentDto.ProductId,
                Rating = commentDto.Rating,
                CommentText = commentDto.CommentText
            };

            await _commentRepository.AddCommentAsync(newComment);
            return newComment;
        }

        public async Task<bool> DeleteCommentAsync(int id, string token)
        {
            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null) return false;

            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null || comment.UserId != userId)
                return false;

            await _commentRepository.DeleteCommentAsync(id);
            return true;
        }
    }
}