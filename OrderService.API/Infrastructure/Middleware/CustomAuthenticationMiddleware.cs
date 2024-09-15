namespace OrderService.API.Infrastructure.Middleware
{
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomAuthenticationMiddleware> _logger;

        public CustomAuthenticationMiddleware(RequestDelegate next, ILogger<CustomAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access attempt. Bearer token is missing or invalid.");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\":\"Unauthorized. Bearer token is missing or invalid.\"}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during authentication");
                throw;
            }
        }
    }

}
