using MedVehicle.Components;
using DotNetEnv;
using MedVehicle.MongoDB;
using MedVehicle.Infrastructure;
using Microsoft.Extensions.FileProviders; // <--- 1. Add this namespace

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Register the DB Context
builder.Services.AddSingleton<NamedCollection, MongoDbContext>();

// 2. Register the Car Service
builder.Services.AddScoped<CarService>();

// 3. Register the File Upload Service
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();


var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseStaticFiles(); 

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();