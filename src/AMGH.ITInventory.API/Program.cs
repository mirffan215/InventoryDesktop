using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Infrastructure.Services;
using AMGH.ITInventory.Persistence;
using AMGH.ITInventory.Persistence.Repositories;
using AMGH.ITInventory.Persistence.Seed;
using AMGH.ITInventory.Security.Jwt;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

var cs = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AmghDbContext>(o => o.UseSqlServer(cs));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAmghInfrastructure(builder.Configuration);
builder.Services.AddAmghSecurity(builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions());

builder.Services.AddControllers().ConfigureApiBehaviorOptions(o =>
    o.InvalidModelStateResponseFactory = ctx => new BadRequestObjectResult(
        AMGH.ITInventory.Shared.Contracts.ApiResponse<object>.Fail("Validation failed",
            ctx.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray())));
builder.Services.AddApiVersioning(o => { o.DefaultApiVersion = new ApiVersion(1, 0); o.AssumeDefaultVersionWhenUnspecified = true; o.ReportApiVersions = true; })
    .AddApiExplorer(o => { o.GroupNameFormat = "'v'VVV"; o.SubstituteApiVersionInUrl = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new() { Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    c.AddSecurityRequirement(new()
    {
        { new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});
builder.Services.AddHealthChecks().AddSqlServer(cs, name: "sqlserver");
builder.Services.AddOpenTelemetry().WithTracing(t => t.AddAspNetCoreInstrumentation().AddConsoleExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation());

var app = builder.Build();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AmghDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}
app.Run();

public partial class Program { }
