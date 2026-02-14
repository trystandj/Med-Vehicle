using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Med_Vehicle.Services; 

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IBrowserFile file, long maxFileSize)
    {
        try
        {
            var rootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(rootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.Name);
            var trustedFileName = $"{Path.GetRandomFileName()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, trustedFileName);

            await using FileStream fs = new(fullPath, FileMode.Create);
            await file.OpenReadStream(maxFileSize).CopyToAsync(fs);

            _logger.LogInformation("Successfully uploaded {Name} to {FullPath}", file.Name, fullPath);
            
            return trustedFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Railway Upload Error for {Name}: {Message}", file.Name, ex.Message);
            return string.Empty; 
        }
    }
}