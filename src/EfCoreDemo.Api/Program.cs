using System.Text.Json.Serialization;
using EfCoreDemo.Api.Endpoints;
using EfCoreDemo.Infrastructure;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=efcoredemo.db";

builder.Services.AddInfrastructure(connectionString);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.SerializerOptions.WriteIndented = true;
});

builder.Services.AddOpenApi();

const string CorsPolicy = "frontend";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)));

var app = builder.Build();

// Aplica migrations e popula o banco no startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseCors(CorsPolicy);
app.MapOpenApi();

app.MapGet("/", () => Results.Redirect("/openapi/v1.json"));

app.MapRelationshipEndpoints();
app.MapLoadingEndpoints();
app.MapQueryEndpoints();
app.MapMutationEndpoints();
app.MapAuditEndpoints();
app.MapMetaEndpoints();

app.Run();
