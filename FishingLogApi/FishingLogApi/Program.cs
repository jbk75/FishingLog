using FishingLogApi.DAL.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

// Add Swagger generation with XML comments support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Enable XML comments (see next step)
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

// Enable middleware to serve swagger-ui (HTML, JS, CSS)
app.UseSwaggerUI();

// Configure the HTTP request pipeline

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();  // replaces UseMvc()

#pragma warning disable S6966 // Awaitable method should be used
app.Run();
#pragma warning restore S6966 // Awaitable method should be used
