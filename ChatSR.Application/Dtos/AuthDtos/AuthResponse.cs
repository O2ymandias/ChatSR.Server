namespace ChatSR.Application.Dtos.AuthDtos;

public record AuthResponse(BasicUserInfo UserInfo, string Token, DateTime ExpiresOn);
public record BasicUserInfo(string UserId, string UserName, string DisplayName, string Email, List<string> Roles);
