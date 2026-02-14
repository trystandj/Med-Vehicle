using Med_Vehicle.Auth;
using Med_Vehicle.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Med_Vehicle.Components;
using DotNetEnv;
using Med_Vehicle.MongoDB;
using Med_Vehicle.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var currentDir = Directory.GetCurrentDirectory();
var webRoot = builder.Environment.WebRootPath;
Console.WriteLine($"[DIAGNOSTIC] Current Directory: {currentDir}");
Console.WriteLine($"[DIAGNOSTIC] WebRootPath: {webRoot}");


var blazorPath = Path.Combine(webRoot ?? "", "_framework", "blazor.web.js");
if (File.Exists(blazorPath))
{
    Console.WriteLine($"[DIAGNOSTIC] ✅ FOUND blazor.web.js at: {blazorPath}");
}
else
{
    Console.WriteLine($"[DIAGNOSTIC] ❌ FILE NOT FOUND at: {blazorPath}");
    Console.WriteLine("[DIAGNOSTIC] Listing all files in WebRoot to find it:");
    if (Directory.Exists(webRoot))
    {
        foreach (var file in Directory.GetFiles(webRoot, "*", SearchOption.AllDirectories))
        {
            Console.WriteLine($" - {file}");
        }
    }
    else
    {
         Console.WriteLine($"[DIAGNOSTIC] ❌ WebRoot directory does not exist!");
    }
}

DotNetEnv.Env.Load();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login"; 
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<UserService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ReminderService>();
builder.Services.AddSingleton<NamedCollection, MongoDbContext>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<VehicleModificationService>();

var app = builder.Build();


app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var proto) 
        && proto == "https")
    {
        context.Request.Scheme = "https";
    }
    await next();
});

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles(); 

var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();