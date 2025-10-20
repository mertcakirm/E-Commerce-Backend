using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;

public interface IQuestionService
{
    Task<ServiceResult<List<GetAllQuestionsDto>>> GetProductQuestions(string token);
    Task<ServiceResult<bool>> AddProductQuestionAsync(int productId, string question, string token);
    Task<ServiceResult<bool>> AddProductAnswerAsync(int questionId, string answer, string token);
    Task<ServiceResult<bool>> RemoveProductQuestionAsync(int questionId, string token);
    Task<ServiceResult<PagedResult<ProductQuestionResponseDto>>> GetProductQuestions(int productId, int pageNumber, int pageSize);
}