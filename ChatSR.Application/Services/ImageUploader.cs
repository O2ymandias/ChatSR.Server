using ChatSR.Application.Interfaces;
using ChatSR.Application.Shared.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChatSR.Application.Services;

public class ImageUploader(
	IOptions<ImageUploaderOptions> options,
	IWebHostEnvironment env
) : IImageUploader
{
	private readonly string[] _allowedExtensions = options.Value.AllowedExtensions;
	private readonly int _maxImageSize = options.Value.MaxFileSizeMB * 1024 * 1024;
	public async Task<string?> UploadImageAsync(IFormFile? file, string folderName)
	{
		if (file is null)
			throw new ArgumentNullException(nameof(file), "No file was provided.");

		if (file.Length == 0)
			throw new ArgumentException("The uploaded file is empty.", nameof(file));

		if (file.Length > _maxImageSize)
		{
			var maxSizeMB = _maxImageSize / 1024 / 1024;
			throw new ArgumentException(
				$"The file size exceeds the maximum allowed size of {maxSizeMB} MB.",
				nameof(file)
			);
		}

		var extension = Path.GetExtension(file.FileName)?.Trim().ToLower();
		if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
		{
			var allowed = string.Join(", ", _allowedExtensions);
			throw new ArgumentException(
				$"Invalid file type. Allowed extensions are: {allowed}.",
				nameof(file)
			);
		}


		var folderPath = Path.Combine(env.WebRootPath, folderName);
		Directory.CreateDirectory(folderPath);

		var fileName = $"{Guid.NewGuid()}{extension}";
		var filePath = Path.Combine(folderPath, fileName);

		using var stream = new FileStream(filePath, FileMode.Create);
		await file.CopyToAsync(stream);

		return Path.Combine(folderName, fileName).Replace("\\", "/");
	}

	public void DeleteFile(string? filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath)) return;

		var path = Path.Combine(env.WebRootPath, filePath);

		if (File.Exists(path)) File.Delete(path);
	}
}
