using Newtonsoft.Json;

namespace user_management.Middlewares;

public class DatabaseExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DatabaseExceptionHandlerMiddleware(RequestDelegate next, IWebHostEnvironment webHostEnvironment)
    {
        _next = next;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            if (e.GetType().Namespace != "MongoDB.Driver") throw;

            int statusCode = 500;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Message = e.Message,
                Exception = SerializeException(e)
            };

            var body = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(body);
        }
    }

    private string? SerializeException(Exception e) => _webHostEnvironment.IsProduction() ? null : e.ToString();
}

public static class DatabaseExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseDatabaseExceptionHandler(this IApplicationBuilder builder) => builder.UseMiddleware<DatabaseExceptionHandlerMiddleware>();
}