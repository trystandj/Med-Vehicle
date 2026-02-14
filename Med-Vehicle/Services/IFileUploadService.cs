using Microsoft.AspNetCore.Components.Forms;

namespace Med_Vehicle.Services;

// Interface for file upload service, defining a method to upload a file and return the generated file name. 
// This abstraction allows for different implementations of file storage (e.g., local, cloud)
//  while keeping the rest of the application decoupled from specific storage details.
public interface IFileUploadService
{
    Task<string> UploadFileAsync(IBrowserFile file, long maxFileSize);
}