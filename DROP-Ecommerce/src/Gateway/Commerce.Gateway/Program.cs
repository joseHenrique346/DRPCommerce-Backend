var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// A integracao entre APIs usa a rede privada e nao deve passar pela entrada publica.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/store/api/internal") ||
        context.Request.Path.StartsWithSegments("/api/drop/api/internal"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next(context);
});

app.MapHealthChecks("/health")
    .AllowAnonymous();

app.MapReverseProxy();

app.Run();
