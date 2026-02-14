using Microsoft.AspNetCore.Components.Forms;

namespace Med_Vehicle.Services;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IBrowserFile file, long maxFileSize);
}