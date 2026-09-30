using FinanceTracker.Components;
using FinanceTracker.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var culture = CultureInfo.GetCultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<FinanceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("FinanceDatabase")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
