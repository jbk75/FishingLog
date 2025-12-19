using FishingLogApi.DAL.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure appsettings.json and environment specific config files
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// ✅ Add named CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:82","http://localhost:81", "http://localhost:15749", "http://localhost:38522") // your frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// 👇 Add Swagger/OpenAPI service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My .NET 8 API",
        Version = "v1",
        Description = "API documentation for my application",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "you@example.com"
        }
    });
});


// Add logging (Console and Debug)
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddScoped<TripRepository>();
builder.Services.AddScoped<VeidistadurRepository>();
builder.Services.AddScoped<FishingPlaceWishlistRepository>();
//builder.Services.AddScoped<VeidiferdirRepository>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();             // 👈 Generate Swagger JSON
    app.UseSwaggerUI(options =>   // 👈 Serve Swagger UI
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My .NET 8 API v1");
        //options.RoutePrefix = string.Empty; // serve Swagger UI at root (https://localhost:5001/)
    });
}

app.UseHttpsRedirection();
// ✅ Enable CORS before controllers
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

#pragma warning disable S6966
app.Run();
#pragma warning restore S6966
