using ChatSR.Application.Dtos.ChatDtos;
using ChatSR.Application.Dtos.ChatMemberDtos;
using ChatSR.Application.Shared.Results;

namespace ChatSR.Application.Interfaces;

public interface IChatService
{
	Task<Result<ChatResponse>> CreateChatAsync(string currentUserId, CreateChatRequest request);
	Task<Result<ChatResponse>> GetChatByIdAsync(string currentUserId, Guid chatId);
	Task<Result<List<ChatListResponse>>> GetUserChatsAsync(string currentUserId);
	Task<Result> AddMembersAsync(string currentUserId, Guid chatId, AddMembersRequest request);
	Task<Result> RemoveMemberAsync(string currentUserId, Guid chatId, string memberIdToRemove);
	Task<Result> LeaveChatAsync(string currentUserId, Guid chatId);
	Task<bool> IsChatMemberAsync(string currentUserId, Guid chatId);
	Task<List<string>> GetChatMemberIdsAsync(Guid chatId);
}
