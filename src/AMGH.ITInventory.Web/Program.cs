using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Infrastructure.Services;
using AMGH.ITInventory.Persistence;
using AMGH.ITInventory.Persistence.Repositories;
using AMGH.ITInventory.Persistence.Seed;
using AMGH.ITInventory.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

builder.Services.AddDbContext<AmghDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAmghInfrastructure(builder.Configuration);

var entraConfigured = !string.IsNullOrWhiteSpace(builder.Configuration["AzureAd:ClientId"]);
if (entraConfigured)
{
    builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd");
}
else if (builder.Environment.IsDevelopment())
{
    // Local development only: auto sign-in as an Admin so the UI is usable without a tenant.
    builder.Services.AddAuthentication(DevAuthHandler.Scheme).AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.Scheme, null);
}
else throw new InvalidOperationException("AzureAd:ClientId must be configured outside Development.");

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("ManageAssets", p => p.RequireRole("Admin", "ITManager", "Technician"));
    o.AddPolicy("ViewReports", p => p.RequireRole("Admin", "ITManager", "Auditor"));
    o.AddPolicy("Administer", p => p.RequireRole("Admin"));
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.MapGet("/health", () => Results.Ok("healthy")).AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AmghDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}
app.Run();
public partial class Program { }
