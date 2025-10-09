using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly UserValidator _userValidator;

        public MessageService(IMessageRepository messageRepository, UserValidator userValidator)
        {
            _messageRepository = messageRepository;
            _userValidator = userValidator;
        }

        public async Task<ServiceResult<bool>> CreateMessageAsync(MessageDto message,string token)
        {
                var validation = await _userValidator.ValidateAsync(token);
                if (validation.IsFail)
                    return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);
                
                if(message==null)return ServiceResult<bool>.Fail("Mesaj içeriğini doldurun." , HttpStatusCode.BadRequest);

                var messageobj = new Message
                {
                    UserId = validation.Data!.Id,
                    MessageText = message.MessageText,
                    MessageTitle = message.MessageTitle,
                    IsReply = false,
                    Answer = ""
                };
            
                var result = await _messageRepository.CreateMessageAsync(messageobj);
                if (!result)
                    return ServiceResult<bool>.Fail("Mesaj oluşturulamadı.");

                return ServiceResult<bool>.Success(true, "Mesaj başarıyla oluşturuldu.");
        }

        public async Task<ServiceResult<PagedResult<MessageResponseDto>>> GetAllMessagesAsync(int pageNumber, int pageSize, string token)
        {
            try
            {
                var validation = await _userValidator.ValidateAsync(token);
                if (validation.IsFail)
                    return ServiceResult<PagedResult<MessageResponseDto>>.Fail(validation.ErrorMessage!, validation.Status);

                var isAdmin = await _userValidator.IsAdminAsync(token);
                if (isAdmin.IsFail || !isAdmin.Data)
                    return ServiceResult<PagedResult<MessageResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

                var allMessages = await _messageRepository.GetAllMessagesAsync();

                var totalCount = allMessages.Count;

                var pagedData = allMessages
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MessageResponseDto
                    {
                        Id = m.Id,
                        UserEmail = m.User?.Email ?? "Bilinmiyor",
                        MessageTitle = m.MessageTitle,
                        MessageText = m.MessageText,
                        IsReply = m.IsReply,
                        Answer = m.Answer
                    })
                    .ToList();

                var pagedResult = new PagedResult<MessageResponseDto>(
                    pagedData,
                    totalCount,
                    pageNumber,
                    pageSize
                );

                return ServiceResult<PagedResult<MessageResponseDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<MessageResponseDto>>.Fail($"Veri alınırken hata oluştu: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ToggleMessageReplyAsync(int messageId, string answer,string token)
        {
            try
            {
                var validation = await _userValidator.ValidateAsync(token);
                if (validation.IsFail)
                    return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

                var isAdmin = await _userValidator.IsAdminAsync(token);
                if (isAdmin.IsFail || !isAdmin.Data)
                    return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
                
                var result = await _messageRepository.ToggleMessageReply(messageId, answer);
                if (!result)
                    return ServiceResult<bool>.Fail("Mesaj bulunamadı veya güncellenemedi.");

                return ServiceResult<bool>.Success(true, "Mesaj başarıyla yanıtlandı.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Hata oluştu: {ex.Message}");
            }
        }
    }
}