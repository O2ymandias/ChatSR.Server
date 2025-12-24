using ChatSR.Api.Extensions;
using ChatSR.Api.Responses;
using ChatSR.Application.Dtos;
using ChatSR.Application.Dtos.ChatMemberDtos;
using ChatSR.Application.Dtos.MessageDtos;
using ChatSR.Application.Interfaces;
using ChatSR.Application.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatSR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MessageController(IMessageService messageService) : ControllerBase
{
	[HttpPost("{chatId:guid}")]
	public async Task<IActionResult> SendMessage(Guid chatId, SendMessageRequest request)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await messageService.SendMessageAsync(currentUserId, chatId, request);
		return response.ToActionResult();
	}

	[HttpGet("{chatId:guid}")]
	public async Task<IActionResult> GetChatMessages(Guid chatId, [FromQuery] PaginationParams pagination)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await messageService.GetChatMessagesAsync(currentUserId, chatId, pagination.Page, pagination.PageSize);
		return response.ToActionResult();
	}

	[HttpPut("{messageId:guid}")]
	public async Task<IActionResult> EditMessage(Guid messageId, EditMessageRequest request)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await messageService.EditMessageAsync(currentUserId, messageId, request);
		return response.ToActionResult();
	}

	[HttpDelete("{messageId:guid}")]
	public async Task<IActionResult> DeleteMessage(Guid messageId)
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (currentUserId is null)
		{
			return BadRequest(ApiResponse<ChatResponse>.Failure(
				Error.Validation("invalid token")
			));
		}

		var response = await messageService.DeleteMessageAsync(currentUserId, messageId);
		return response.ToActionResult();
	}
}
