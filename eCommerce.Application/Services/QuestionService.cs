using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IProductRepository _productRepository;
    private readonly UserValidator _userValidator;

    public QuestionService(IProductRepository productRepository, UserValidator userValidator)
    {
        _productRepository = productRepository;
        _userValidator = userValidator;
    }
    
    public async Task<ServiceResult<List<GetAllQuestionsDto>>> GetProductQuestions(string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult<List<GetAllQuestionsDto>>.Fail(validation.ErrorMessage!, validation.Status);

        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<List<GetAllQuestionsDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        var result = await _productRepository.GetProductQuestions();
        if (result == null || !result.Any()) return ServiceResult<List<GetAllQuestionsDto>>.Fail("Sorular bulunamadı!", HttpStatusCode.NotFound);
        
        var questionsDto = result.Select(q => new GetAllQuestionsDto
        {
            Id = q.Id,
            ProductId = q.Product.Id,
            ProductName = q.Product.Name,
            UserName = q.User.Name,
            QuestionText = q.QuestionText,
            IsCorrect = q.IsCorrect,
            CreatedDate = q.CreatedAt
        }).ToList();
        
        return ServiceResult<List<GetAllQuestionsDto>>.Success(questionsDto);
        
    }

    public async Task<ServiceResult<bool>> AddProductQuestionAsync(int productId, string question, string token)
    {
        
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;
        
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return ServiceResult<bool>.Fail("Product not found",HttpStatusCode.NotFound);

        var added = await _productRepository.AddProductQuestion(productId,question, userId);

        return ServiceResult<bool>.Success(added);
    }
    
    public async Task<ServiceResult<bool>> AddProductAnswerAsync(int questionId, string answer, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        var added = await _productRepository.AddProductAnswer( questionId , answer , userId );

        return ServiceResult<bool>.Success(added);
    }

    public async Task<ServiceResult<bool>> RemoveProductQuestionAsync(int questionId, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        if (questionId == null) return ServiceResult<bool>.Fail("Soru idsi boş!", HttpStatusCode.Forbidden);

        await _productRepository.DeleteProductQuestion(questionId);
        return ServiceResult<bool>.Success(true);
    }
    
}