using ChatSR.Application.Dtos.MessageDtos;
using ChatSR.Application.Shared.Results;

namespace ChatSR.Application.Interfaces;

public interface IMessageService
{
	Task<Result<MessageResponse>> SendMessageAsync(string currentUserId, Guid chatId, SendMessageRequest request);
	Task<PagedResult<MessageResponse>> GetChatMessagesAsync(string currentUserId, Guid chatId, int page, int pageSize);
	Task<Result<MessageResponse>> EditMessageAsync(string currentUserId, Guid messageId, EditMessageRequest request);
	Task<Result<bool>> DeleteMessageAsync(string currentUserId, Guid messageId);
}
