using System.Net;
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
        private readonly IProductRepository  _productRepository;

        public CommentService(ICommentRepository commentRepository, ITokenService tokenService, IProductRepository  productRepository)
        {
            _commentRepository = commentRepository;
            _tokenService = tokenService;
            _productRepository = productRepository;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task<ServiceResult<PagedResult<CommentListDto>>> GetCommentsByProductIdAsync(int productId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var comments = await _commentRepository.GetCommentsByProductIdAsync(productId);

            var commentsDto = comments.Select(c => new CommentListDto
            {
                Id = c.Id,
                CommentText = c.CommentText,
                Rating = c.Rating,
                UserName = c.User != null ? c.User.Name : null
            }).ToList();

            var totalCount = commentsDto.Count;

            var pagedComments = commentsDto
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new PagedResult<CommentListDto>(pagedComments, totalCount, pageNumber, pageSize);

            return ServiceResult<PagedResult<CommentListDto>>.Success(pagedResult);
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

            var product = await _productRepository.GetByIdAsync(commentDto.ProductId);
            if (product != null)
            {
                var comments = await _commentRepository.GetCommentsByProductIdAsync(commentDto.ProductId);
                product.AverageRating = comments.Any() 
                    ? comments.Average(c => c.Rating) 
                    : 0.0;

                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();
            }

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

            var product = await _productRepository.GetByIdAsync(comment.ProductId);
            if (product != null)
            {
                var comments = await _commentRepository.GetCommentsByProductIdAsync(comment.ProductId);
                product.AverageRating = comments.Any() 
                    ? comments.Average(c => c.Rating) 
                    : 0.0;
                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();
            }

            return true;
        }
    }
}