using LiteDB;
using api.Repositories;
using api.Services;
using api.Dtos;
using api.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Configure LiteDB and repository
var databaseFileName = builder.Configuration["GameOfLife:DatabasePath"] ?? "game_of_life.db";
var databasePath = Path.Combine(AppContext.BaseDirectory, databaseFileName);
builder.Services.AddSingleton(_ => new LiteDatabase($"Filename={databasePath};Mode=Exclusive"));
builder.Services.AddScoped<IBoardRepository, LiteBoardRepository>();

// Register Game of Life service
builder.Services.AddScoped<IGameOfLifeService, GameOfLifeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();

/// <summary>
/// Global exception handling middleware to catch and format exceptions consistently.
/// </summary>
public class GlobalExceptionHandlingMiddleware {
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception) {
        context.Response.ContentType = "application/json";

        var response = exception switch {
            BoardNotFoundException ex => new {
                statusCode = StatusCodes.Status404NotFound,
                errorResponse = new ErrorResponse(404, ex.Message)
            },
            InvalidBoardStateException ex => new {
                statusCode = StatusCodes.Status400BadRequest,
                errorResponse = new ErrorResponse(400, ex.Message)
            },
            InvalidStepsException ex => new {
                statusCode = StatusCodes.Status400BadRequest,
                errorResponse = new ErrorResponse(400, ex.Message)
            },
            NoFinalStateException ex => new {
                statusCode = StatusCodes.Status422UnprocessableEntity,
                errorResponse = new ErrorResponse(422, ex.Message)
            },
            _ => new {
                statusCode = StatusCodes.Status500InternalServerError,
                errorResponse = new ErrorResponse(500, "An unexpected error occurred.")
            }
        };

        context.Response.StatusCode = response.statusCode;
        return context.Response.WriteAsJsonAsync(response.errorResponse);
    }
}
