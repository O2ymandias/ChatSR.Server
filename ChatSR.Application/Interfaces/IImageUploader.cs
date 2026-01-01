using Microsoft.AspNetCore.Http;

namespace ChatSR.Application.Interfaces;

public interface IImageUploader
{
	Task<string?> UploadImageAsync(IFormFile? file, string folderName);
	void DeleteFile(string? filePath);
}
