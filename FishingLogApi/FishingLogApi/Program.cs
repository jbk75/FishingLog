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

// Add Swagger generation with XML comments support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add logging (Console and Debug)
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddScoped<VeidiferdirRepository>();
builder.Services.AddScoped<VeidistadurRepository>();
builder.Services.AddScoped<VeidiferdirRepository>();

var app = builder.Build();

// Enable middleware to serve generated Swagger as JSON endpoint
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Enable CORS before controllers
app.UseCors("AllowFrontend");

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

#pragma warning disable S6966
app.Run();
#pragma warning restore S6966
