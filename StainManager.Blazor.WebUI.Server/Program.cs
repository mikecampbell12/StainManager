using Cropper.Blazor.Extensions;
using MudBlazor.Services;
using StainManager.Blazor.WebUI.Server;
using StainManager.Blazor.WebUI.Server.Features.Shared.Services;
using StainManager.Blazor.WebUI.Server.Features.Species.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCropper();
builder.Services.AddMudServices();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    // logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
    // logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
});

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // Set the maximum message size to 10MB
});

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseAddress"] ?? string.Empty)
});

builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<ITempImageService, TempImageService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();