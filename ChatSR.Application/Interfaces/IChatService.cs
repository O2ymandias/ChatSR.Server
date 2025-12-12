using ChatSR.Application.Dtos.ChatDtos;
using ChatSR.Application.Dtos.ChatMemberDtos;
using ChatSR.Application.Shared.Results;

namespace ChatSR.Application.Interfaces;

public interface IChatService
{
	Task<Result<ChatResponse>> CreateChatAsync(string currentUserId, CreateChatRequest request);
	Task<Result<ChatResponse>> GetChatByIdAsync(string currentUserId, Guid chatId);
	Task<Result<List<ChatListResponse>>> GetUserChatsAsync(string currentUserId);
	Task<Result<ChatResponse>> AddMembersAsync(string currentUserId, Guid chatId, AddMembersRequest request);
	Task<Result<ChatResponse>> RemoveMemberAsync(string currentUserId, Guid chatId, string memberIdToRemove);
	Task<Result<ChatActionResponse>> LeaveChatAsync(string currentUserId, Guid chatId);
}
