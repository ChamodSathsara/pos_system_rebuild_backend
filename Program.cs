using System.Text.Json.Serialization;
using PosApi.Extensions;
using PosApi.Middleware;
using Serilog;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// QuestPDF (used by the Sales Reports PDF exporters) - Community license
// is free for small companies; see https://www.questpdf.com/license
// ---------------------------------------------------------------------
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---------------------------------------------------------------------
// Logging
// ---------------------------------------------------------------------
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// ---------------------------------------------------------------------
// Controllers / JSON
// ---------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ---------------------------------------------------------------------
// Infrastructure / application services
// ---------------------------------------------------------------------
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddValidatorsAndFluentValidation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
        }
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// ---------------------------------------------------------------------
// Apply pending migrations and seed baseline data (roles + default admin).
// Controlled via configuration so it can be disabled in environments where
// migrations are applied out-of-band (e.g. CI/CD release pipelines).
// ---------------------------------------------------------------------
if (app.Configuration.GetValue("ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PosApi.Data.ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PosApi.Security.IPasswordHasher>();
    await PosApi.Data.DbSeeder.SeedAsync(dbContext, passwordHasher);
}

// ---------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "v1/swagger.json",
            "POS System API v1"
        );
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed for WebApplicationFactory-based integration testing.
public partial class Program { }
