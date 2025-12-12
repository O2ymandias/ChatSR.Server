using ChatSR.Api.Extensions;
using ChatSR.Api.Responses;
using ChatSR.Application.Dtos.ChatMemberDtos;
using ChatSR.Application.Interfaces;
using ChatSR.Application.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ChatSR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> CreateChat(CreateChatRequest request)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await chatService.CreateChatAsync(currentUserId, request);
		return response.ToActionResult();
	}

	[HttpGet("{chatId:guid}")]
	public async Task<IActionResult> GetChatById(Guid chatId)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await chatService.GetChatByIdAsync(currentUserId, chatId);
		return response.ToActionResult();
	}

	[HttpGet("user-chats")]
	public async Task<IActionResult> GetUserChats()
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await chatService.GetUserChatsAsync(currentUserId);
		return response.ToActionResult();
	}

	[HttpPut("add-members/{chatId:guid}")]
	public async Task<IActionResult> AddMembersToChat(Guid chatId, AddMembersRequest request)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await chatService.AddMembersAsync(currentUserId, chatId, request);
		return response.ToActionResult();
	}

	[HttpDelete("remove-member/{chatId:guid}")]
	public async Task<IActionResult> RemoveMemberFromChat(Guid chatId, [Required] string memberId)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}
		var response = await chatService.RemoveMemberAsync(currentUserId, chatId, memberId);
		return response.ToActionResult();
	}

	[HttpGet("leave-chat/{chatId:guid}")]
	public async Task<IActionResult> LeaveChat(Guid chatId)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await chatService.LeaveChatAsync(currentUserId, chatId);
		return response.ToActionResult();
	}
}
