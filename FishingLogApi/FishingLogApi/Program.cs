using FishingLogApi.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure appsettings.json and environment specific config files
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container
builder.Services.AddCors();

builder.Services.AddControllers();  // Modern replacement for AddMvc()

// Add logging (Console and Debug)
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddScoped<VeidiferdirRepository>();
builder.Services.AddScoped<VeidistadurRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseStaticFiles();

app.MapControllers();  // replaces UseMvc()

#pragma warning disable S6966 // Awaitable method should be used
app.Run();
#pragma warning restore S6966 // Awaitable method should be used
