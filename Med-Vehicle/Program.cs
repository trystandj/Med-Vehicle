using Med_Vehicle.Auth;
using Med_Vehicle.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Med_Vehicle.Components;

using DotNetEnv;
using Med_Vehicle.MongoDB;
using Med_Vehicle.Infrastructure;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState(); // Importante para .NET 10
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddSingleton<UserService>();



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// builder.Services.AddScoped<EmailService>(); Uncommit when email service is needed

// 1. Register the DB Context
builder.Services.AddSingleton<NamedCollection, MongoDbContext>();

// 2. Register the Car Service
builder.Services.AddScoped<CarService>();

// 3. Register the File Upload Service
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
