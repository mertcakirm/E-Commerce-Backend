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
        private readonly IProductRepository  _productRepository;
        private readonly UserValidator _userValidator;

        public CommentService(ICommentRepository commentRepository , IProductRepository  productRepository, UserValidator userValidator)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
            _userValidator = userValidator;
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
        
        public async Task<ServiceResult<Comment?>> AddCommentAsync(CommentCreateDto commentDto, string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) return ServiceResult<Comment?>.Fail(validation.ErrorMessage!, validation.Status);

            var userId = validation.Data!.Id;

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

                await _productRepository.UpdateAsync(product);
                await _productRepository.SaveChangesAsync();
            }

            return ServiceResult<Comment?>.Success(newComment, "Yorum eklendi");
        }

        public async Task<ServiceResult<bool>> DeleteCommentAsync(int id, string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

            var userId = validation.Data!.Id;

            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null || comment.UserId != userId) return ServiceResult<bool>.Fail("Yorum bulunamadı veya yetkisiz", HttpStatusCode.NotFound);

            await _commentRepository.DeleteCommentAsync(id);

            var product = await _productRepository.GetByIdAsync(comment.ProductId);
            if (product != null)
            {
                var comments = await _commentRepository.GetCommentsByProductIdAsync(comment.ProductId);
                product.AverageRating = comments.Any() 
                    ? comments.Average(c => c.Rating) 
                    : 0.0;
                await _productRepository.UpdateAsync(product);
                await _productRepository.SaveChangesAsync();
            }

            return ServiceResult<bool>.Success(true, "Yorum silindi");
        }
    }
}