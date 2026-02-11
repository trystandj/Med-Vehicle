using Microsoft.AspNetCore.Components.Forms;

//  Service promises that if you give it a file ans size limit, it will store the file and return the stored filename
public interface IFileUploadService
{
    Task<string> UploadFileAsync(IBrowserFile file, long maxFileSize);
}


public class FileUploadService : IFileUploadService
{
    // enviornment to get wwwroot path
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;

    // Constructor
    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    // Uploads the file and returns the stored filename
    public async Task<string> UploadFileAsync(IBrowserFile file, long maxFileSize)
    {
        try
        {
            // Ignore the original filename for security reasons and use a random name
            var trustedFileName = Path.GetRandomFileName();
            //  builds the full path to the uploads folder
            var path = Path.Combine(_environment.WebRootPath, "uploads", trustedFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // Steaming the file to disk or saving it. Copytoasync flows the data of the file between the web server and disk
            await using FileStream fs = new(path, FileMode.Create);
            await file.OpenReadStream(maxFileSize).CopyToAsync(fs);

            _logger.LogInformation("Uploaded file: {Name} as {StoredName}", file.Name, trustedFileName);
            
            // Return the stored filename to the caller
            return trustedFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error uploading {Name}: {Message}", file.Name, ex.Message);
            throw; 
        }
    }
}